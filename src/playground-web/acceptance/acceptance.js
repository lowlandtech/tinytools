// Browser acceptance for the published playground. Serves the publish output over
// HTTP at the path the Pages deployment uses, drives it with Playwright's Chromium and
// checks each acceptance criterion of SPEC3326-US01-SC01 against observed browser state.
//
// Usage, after `dotnet publish src/playground-web -c Release -o <publish>`:
//   npm install
//   node acceptance.js --publish <publish>/wwwroot --out ./artifacts
//
// Environment: CHROME_PATH may point at a Chromium executable; otherwise Playwright's
// own download is used. The script never touches a user browser profile.

const http = require('http');
const fs = require('fs');
const os = require('os');
const path = require('path');
const { chromium } = require('playwright-core');

const args = new Map();
for (let i = 2; i < process.argv.length; i += 2) {
  args.set(process.argv[i].replace(/^--/, ''), process.argv[i + 1]);
}

const publish = path.resolve(args.get('publish') || path.join('..', 'bin', 'Release', 'net10.0', 'publish', 'wwwroot'));
const out = path.resolve(args.get('out') || path.join(os.tmpdir(), 'tinytools-playground-acceptance'));
const port = Number(args.get('port') || 5199);
const prefix = '/playground';

const types = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.mjs': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.wasm': 'application/wasm',
  '.webmanifest': 'application/manifest+json',
  '.ico': 'image/x-icon',
  '.png': 'image/png',
  '.svg': 'image/svg+xml',
  '.txt': 'text/plain; charset=utf-8',
};

function serve(missing) {
  const server = http.createServer((request, response) => {
    const url = decodeURIComponent(request.url.split('?')[0]);
    if (!url.startsWith(prefix)) {
      missing.push('404 ' + url);
      response.writeHead(404).end('outside the playground path: ' + url);
      return;
    }
    let relative = url.slice(prefix.length);
    if (relative === '' || relative === '/') relative = '/index.html';
    const file = path.join(publish, relative);
    if (!file.startsWith(publish)) {
      response.writeHead(403).end('forbidden');
      return;
    }
    fs.readFile(file, (error, data) => {
      if (error) {
        missing.push('404 ' + relative);
        response.writeHead(404, { 'content-type': 'text/plain' }).end('not found: ' + relative);
        return;
      }
      response.writeHead(200, {
        'content-type': types[path.extname(file).toLowerCase()] || 'application/octet-stream',
        'cache-control': 'no-store',
      }).end(data);
    });
  });
  return new Promise((resolve) => server.listen(port, '127.0.0.1', () => resolve(server)));
}

const checks = [];
function check(criterion, name, passed, observed) {
  checks.push({ criterion, name, passed, observed });
}

// The clipboard round-trips through the Windows clipboard, which normalises to CRLF, so
// comparisons against DOM textContent have to ignore the line-ending difference.
const sameText = (left, right) => left.replace(/\r\n/g, '\n') === right.replace(/\r\n/g, '\n');

async function main() {
  if (!fs.existsSync(path.join(publish, 'index.html'))) {
    throw new Error('no publish output at ' + publish + ' - pass --publish <dir>/wwwroot');
  }
  fs.mkdirSync(out, { recursive: true });

  const missing = [];
  const server = await serve(missing);
  const base = `http://127.0.0.1:${port}${prefix}/`;
  const browser = await chromium.launch({ executablePath: process.env.CHROME_PATH || undefined, headless: true });
  const context = await browser.newContext();
  await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: `http://127.0.0.1:${port}` });
  const page = await context.newPage();

  const consoleErrors = [];
  const badResponses = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });
  page.on('pageerror', (error) => consoleErrors.push(String(error)));
  page.on('response', (response) => {
    if (response.status() >= 400) badResponses.push(response.status() + ' ' + response.url());
  });

  await page.goto(base, { waitUntil: 'domcontentloaded' });

  // UAC01 - the engine runs from the WebAssembly build and reports its assembly version.
  await page.waitForFunction(
    () => (document.querySelector('pre.output')?.textContent || '').trim().length > 0,
    null,
    { timeout: 90000 }
  );
  const rendered = await page.locator('pre.output').textContent();
  const version = (await page.locator('.version').textContent()).trim();
  const wasm = await page.evaluate(() =>
    performance.getEntriesByType('resource').map((entry) => entry.name).filter((name) => name.endsWith('.wasm'))
  );
  const engineWasm = wasm.filter((name) => name.includes('LowlandTech.TinyTools.'));
  const frameworkLoaded = await page.evaluate(
    () => performance.getEntriesByType('resource').filter((entry) => entry.name.includes('_framework/')).length
  );
  const frameworkPublished = fs.readdirSync(path.join(publish, '_framework')).length;

  check('UAC01', 'default render', rendered === 'Hello Ada!', rendered);
  check('UAC01', 'engine version from the assembly', /^Engine \d+\.\d+\.\d+\.\d+$/.test(version), version);
  check('UAC01', 'engine assembly fetched as WebAssembly', engineWasm.length > 0, engineWasm.map((n) => path.basename(n)));

  // UAC02 - editing template or model re-renders with the engine's exact output. A JSON key
  // named Count also proves dictionary entries resolve before CLR members in the browser build.
  await page.locator('#input-model').fill('{"Name":"Grace","Count":7}');
  await page.locator('#input-template').fill('Hi ${Context.Model.Name}, you have ${Context.Model.Count} item(s).');
  await page.locator('#render').click();
  await page.waitForFunction(
    () => (document.querySelector('pre.output')?.textContent || '').includes('Grace'),
    null,
    { timeout: 20000 }
  );
  const edited = await page.locator('pre.output').textContent();
  check('UAC02', 'edited template and model re-render', edited === 'Hi Grace, you have 7 item(s).', edited);

  // UAC03 - a malformed template reports the engine diagnostic code with its position.
  await page.locator('#input-template').fill('Hello ${Context.Model.Name');
  await page.locator('#render').click();
  await page.waitForSelector('p.diagnostic', { timeout: 20000 });
  const diagnostic = (await page.locator('p.diagnostic').textContent()).trim();
  const diagnosticText = await page.locator('pre.output').textContent();
  check('UAC03', 'diagnostic code shown', /^TS\d{3}$/.test(diagnostic), diagnostic);
  check('UAC03', 'diagnostic carries line and column', /line 1, column 7/.test(diagnosticText), diagnosticText);

  // UAC04 - C#, workflow YAML and CLI snippets, each copied as runnable text.
  await page.locator('#input-template').fill('Hello ${Context.Model.Name}, you have ${Context.Model.Count} item(s).');
  await page.locator('#render').click();
  await page.waitForFunction(() => !document.querySelector('p.diagnostic'), null, { timeout: 20000 });

  const snippetText = async (id) => (await page.locator(id).textContent()).trim();
  const snippets = {
    csharp: await snippetText('#snippet-csharp'),
    workflow: await snippetText('#snippet-workflow'),
    cli: await snippetText('#snippet-cli'),
  };
  check('UAC04', 'C# snippet names the engine', snippets.csharp.includes('TinyTemplateEngine'), snippets.csharp.split('\n')[0]);
  check('UAC04', 'workflow snippet declares a template step', /type: template/.test(snippets.workflow), snippets.workflow.split('\n')[1]);
  check('UAC04', 'CLI snippet is runnable', /^tinytools render \S+ --model \S+/.test(snippets.cli), snippets.cli);

  const copied = {};
  for (const [name, selector] of [
    ['csharp', '#copy-csharp'],
    ['workflow', '#copy-workflow'],
    ['cli', '#copy-cli'],
  ]) {
    await page.locator(selector).click();
    await page.waitForFunction(
      () => (document.querySelector('p.copy-status')?.textContent || '').includes('Copied'),
      null,
      { timeout: 20000 }
    );
    copied[name] = await page.evaluate(() => navigator.clipboard.readText());
  }
  check('UAC04', 'C# copy action matches the snippet', sameText(copied.csharp, snippets.csharp), copied.csharp.split('\n')[0]);
  check('UAC04', 'workflow copy action matches the snippet', sameText(copied.workflow, snippets.workflow), copied.workflow.split('\n')[1]);
  check('UAC04', 'CLI copy action matches the snippet', sameText(copied.cli, snippets.cli), copied.cli);

  // UAC05 - the playground route loads only what it needs: the React site carries no
  // playground bytes, and this document fetched a fraction of the published framework files.
  check(
    'UAC05',
    'framework payload is fetched on demand',
    frameworkLoaded > 0 && frameworkLoaded < frameworkPublished,
    `${frameworkLoaded} of ${frameworkPublished} framework files fetched on load`
  );

  await page.screenshot({ path: path.join(out, 'playground.png'), fullPage: true });
  await browser.close();
  server.close();

  const evidence = {
    url: base,
    publish,
    version,
    checks,
    consoleErrors,
    badResponses,
    missing,
    frameworkLoaded,
    frameworkPublished,
    screenshots: [path.join(out, 'playground.png')],
  };
  fs.writeFileSync(path.join(out, 'evidence.json'), JSON.stringify(evidence, null, 2));

  const failed = checks.filter((entry) => !entry.passed);
  for (const entry of checks) {
    console.log(`${entry.passed ? 'PASS' : 'FAIL'} ${entry.criterion} ${entry.name}: ${JSON.stringify(entry.observed)}`);
  }
  if (badResponses.length > 0) console.log('HTTP >= 400: ' + JSON.stringify(badResponses));
  if (missing.length > 0) console.log('requests the publish output could not answer: ' + JSON.stringify(missing));
  if (consoleErrors.length > 0) console.log('console errors: ' + JSON.stringify(consoleErrors));
  console.log(`${checks.length - failed.length}/${checks.length} checks passed -> ${out}`);
  process.exitCode = failed.length === 0 ? 0 : 1;
}

main().catch((error) => {
  console.error(String(error));
  process.exitCode = 1;
});
