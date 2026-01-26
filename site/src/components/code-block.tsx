import { useEffect, useState } from "react";
import { codeToHtml } from "shiki";
import { cn } from "@/lib/utils";

interface CodeBlockProps {
  code: string;
  language?: string;
  className?: string;
}

export function CodeBlock({ code, language = "csharp", className }: CodeBlockProps) {
  const [html, setHtml] = useState<string>("");

  useEffect(() => {
    codeToHtml(code.trim(), {
      lang: language,
      theme: "github-dark",
    }).then(setHtml);
  }, [code, language]);

  return (
    <div
      className={cn(
        "rounded-lg overflow-hidden text-sm [&_pre]:p-4 [&_pre]:overflow-x-auto",
        className
      )}
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );
}
