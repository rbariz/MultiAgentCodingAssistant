import type { GeneratedFile } from "@/types/generation";
import { useState, useEffect } from "react";
import { FileCode2 } from "lucide-react";
import Editor from "@monaco-editor/react";
import { cn } from "@/lib/utils";

export function GeneratedFilesExplorer({ files }: { files: GeneratedFile[] }) {
  const [active, setActive] = useState<string | null>(null);

  useEffect(() => {
    if (!active && files.length) {
      setActive(files[0].path);
    }
  }, [files, active]);

  const current = files.find((f) => f.path === active);

  return (
    <div className="rounded-2xl border border-border bg-card p-4">
      <h3 className="mb-3 text-sm font-semibold uppercase tracking-wider text-muted-foreground">
        Generated Files ({files.length})
      </h3>

      {files.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-6 text-center text-sm text-muted-foreground">
          Files will appear here as agents generate them.
        </div>
      ) : (
        <div className="grid gap-3 md:grid-cols-[240px_1fr]">
          <div className="max-h-[560px] overflow-y-auto rounded-lg border border-border bg-background/40 p-1">
            {files.map((f) => (
              <button
                key={f.path}
                onClick={() => setActive(f.path)}
                className={cn(
                  "flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-left text-xs font-mono transition",
                  active === f.path
                    ? "bg-primary/15 text-foreground"
                    : "text-muted-foreground hover:bg-secondary",
                )}
              >
                <FileCode2 className="h-3.5 w-3.5 shrink-0" />
                <span className="truncate">{f.path}</span>
              </button>
            ))}
          </div>

          <div className="overflow-hidden rounded-lg border border-border bg-[oklch(0.1_0.02_265)]">
            <div className="flex items-center justify-between border-b border-border bg-background/60 px-3 py-2">
              <span className="font-mono text-xs text-muted-foreground">
                {current?.path ?? "No file selected"}
              </span>

              <span className="rounded-full bg-secondary px-2 py-0.5 text-[11px] text-muted-foreground">
                {current ? inferLanguage(current.path) : "plaintext"}
              </span>
            </div>

            <Editor
              height="520px"
              theme="vs-dark"
              language={current ? inferLanguage(current.path) : "plaintext"}
              value={current?.content || "(empty)"}
              options={{
                readOnly: true,
                minimap: { enabled: false },
                fontSize: 13,
                wordWrap: "on",
                scrollBeyondLastLine: false,
                automaticLayout: true,
                smoothScrolling: true,
                padding: { top: 14, bottom: 14 },
              }}
            />
          </div>
        </div>
      )}
    </div>
  );
}

function inferLanguage(path: string) {
  const fileName = path.split("/").pop()?.toLowerCase() ?? "";

  if (fileName === "dockerfile") return "dockerfile";
  if (fileName === ".gitignore") return "plaintext";
  if (fileName === "license") return "plaintext";

  const ext = path.split(".").pop()?.toLowerCase();

  switch (ext) {
    case "html":
      return "html";
    case "css":
      return "css";
    case "js":
      return "javascript";
    case "ts":
      return "typescript";
    case "json":
      return "json";
    case "md":
      return "markdown";
    case "cs":
      return "csharp";
    case "razor":
      return "razor";
    case "yml":
    case "yaml":
      return "yaml";
    default:
      return "plaintext";
  }
}