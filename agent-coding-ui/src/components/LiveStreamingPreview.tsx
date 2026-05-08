import { useState, useEffect } from "react";

export function LiveStreamingPreview({ streams }: { streams: Record<string, string> }) {
  const files = Object.keys(streams);
  const [active, setActive] = useState<string | null>(null);

  useEffect(() => {
    if (!active && files.length) setActive(files[0]);
    if (active && !files.includes(active) && files.length) setActive(files[0]);
  }, [files, active]);

  return (
    <div className="rounded-2xl border border-border bg-card p-4">
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-sm font-semibold uppercase tracking-wider text-muted-foreground">Live Streaming Preview</h3>
        {files.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {files.map((f) => (
              <button
                key={f}
                onClick={() => setActive(f)}
                className={`rounded-md px-2 py-0.5 font-mono text-xs ${
                  active === f
                    ? "bg-primary text-primary-foreground"
                    : "bg-secondary text-secondary-foreground"
                }`}
              >
                {f}
              </button>
            ))}
          </div>
        )}
      </div>
      <pre className="h-56 overflow-auto rounded-lg bg-[oklch(0.1_0.02_265)] p-3 font-mono text-xs text-foreground">
        {active ? streams[active] || "" : "No streams yet."}
      </pre>
    </div>
  );
}
