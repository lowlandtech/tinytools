using LowlandTech.TinyTools.Playground;
using LowlandTech.TinyTools.Playground.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<PlaygroundSession>();

await builder.Build().RunAsync();
