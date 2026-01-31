import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Layout } from "@/components/layout";
import { HomePage } from "@/pages/home";
import { GettingStartedPage } from "@/pages/getting-started";
import { TemplateEnginePage } from "@/pages/template-engine";
import { VariableResolverPage } from "@/pages/variable-resolver";
import { ExecutionContextPage } from "@/pages/execution-context";
import { ApiReferencePage } from "@/pages/api-reference";
import { ExamplesPage } from "@/pages/examples";
import { CodeGenerationPage } from "@/pages/code-generation";
import { ITemplatePage } from "@/pages/itemplate";

function App() {
  return (
    <BrowserRouter>
      <Layout>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/getting-started" element={<GettingStartedPage />} />
          <Route path="/template-engine" element={<TemplateEnginePage />} />
          <Route path="/variable-resolver" element={<VariableResolverPage />} />
          <Route path="/execution-context" element={<ExecutionContextPage />} />
          <Route path="/examples" element={<ExamplesPage />} />
          <Route path="/code-generation" element={<CodeGenerationPage />} />
          <Route path="/template" element={<ITemplatePage />} />
          <Route path="/api-reference" element={<ApiReferencePage />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  );
}

export default App;
