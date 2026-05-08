import type { AgentRole, StepStatus } from "@/types/generation";
import { CheckCircle2, Loader2, Circle, XCircle, Brain, Compass, Code2, ShieldCheck } from "lucide-react";
import { cn } from "@/lib/utils";

const AGENTS: { role: AgentRole; icon: any; description: string }[] = [
  { role: "Planner", icon: Brain, description: "Decomposes the goal into steps" },
  { role: "Architect", icon: Compass, description: "Designs file structure & stack" },
  { role: "Developer", icon: Code2, description: "Writes the source code" },
  { role: "Reviewer", icon: ShieldCheck, description: "Audits the final output" },
];

interface Props {
  statuses: Record<AgentRole, StepStatus>;
}

function StatusIcon({ status }: { status: StepStatus }) {
  if (status === "Running") return <Loader2 className="h-5 w-5 animate-spin text-primary" />;
  if (status === "Completed") return <CheckCircle2 className="h-5 w-5" style={{ color: "var(--success)" }} />;
  if (status === "Failed") return <XCircle className="h-5 w-5 text-destructive" />;
  return <Circle className="h-5 w-5 text-muted-foreground" />;
}

export function AgentTimeline({ statuses }: Props) {
  return (
    <div className="rounded-2xl border border-border bg-card p-5">
      <h3 className="mb-4 text-sm font-semibold uppercase tracking-wider text-muted-foreground">Agent Timeline</h3>
      <div className="grid gap-3 md:grid-cols-4">
        {AGENTS.map(({ role, icon: Icon, description }) => {
          const status = statuses[role] ?? "Pending";
          return (
            <div
              key={role}
              className={cn(
                "rounded-xl border p-4 transition",
                status === "Running" && "border-primary bg-primary/5",
                status === "Completed" && "border-border bg-secondary/40",
                status === "Failed" && "border-destructive/60 bg-destructive/5",
                status === "Pending" && "border-border bg-background/40",
              )}
            >
              <div className="mb-2 flex items-center justify-between">
                <Icon className="h-5 w-5 text-primary" />
                <StatusIcon status={status} />
              </div>
              <div className="text-sm font-semibold text-foreground">{role}</div>
              <div className="mt-1 text-xs text-muted-foreground">{description}</div>
              <div className="mt-2 text-[10px] font-mono uppercase text-muted-foreground">{status}</div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
