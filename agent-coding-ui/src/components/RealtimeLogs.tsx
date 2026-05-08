import type { LogEntry } from "@/types/generation";
import { useEffect, useRef } from "react";
import { cn } from "@/lib/utils";

export function RealtimeLogs({ logs }: { logs: LogEntry[] }) {
  const ref = useRef<HTMLDivElement>(null);
  useEffect(() => {
    if (ref.current) ref.current.scrollTop = ref.current.scrollHeight;
  }, [logs]);

  return (
    <div className="rounded-2xl border border-border bg-card p-4">
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-sm font-semibold uppercase tracking-wider text-muted-foreground">Realtime Logs</h3>
        <span className="text-xs text-muted-foreground">{logs.length} events</span>
      </div>
      <div
        ref={ref}
        className="h-64 overflow-y-auto rounded-lg bg-[oklch(0.1_0.02_265)] p-3 font-mono text-xs"
      >
        {logs.length === 0 && <div className="text-muted-foreground">Waiting for events…</div>}
        {logs.map((l) => (
          <div key={l.id} className="flex gap-2 py-0.5">
            <span className="text-muted-foreground">{l.timestamp}</span>
            <span
              className={cn(
                l.level === "info" && "text-[oklch(0.8_0.02_260)]",
                l.level === "success" && "text-[oklch(0.8_0.18_150)]",
                l.level === "error" && "text-[oklch(0.75_0.2_25)]",
                l.level === "agent" && "text-[oklch(0.8_0.18_265)]",
              )}
            >
              {l.message}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
