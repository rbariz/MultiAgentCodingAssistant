import { createFileRoute } from "@tanstack/react-router";
import { useCallback, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { Sparkles, Activity } from "lucide-react";
import { PromptComposer } from "@/components/PromptComposer";
import { AgentTimeline } from "@/components/AgentTimeline";
import { RealtimeLogs } from "@/components/RealtimeLogs";
import { LiveStreamingPreview } from "@/components/LiveStreamingPreview";
import { GeneratedFilesExplorer } from "@/components/GeneratedFilesExplorer";
import { ActionsPanel } from "@/components/ActionsPanel";
import { api } from "@/lib/api";
import { createGenerationConnection, type RealtimeEventName } from "@/lib/realtime";
import type {
  AgentRole,
  GeneratedFile,
  Generation,
  LogEntry,
  StepStatus,
} from "@/types/generation";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "Multi-Agent Coding Assistant" },
      { name: "description", content: "Generate full projects with a team of AI engineering agents." },
    ],
  }),
  component: Dashboard,
});

const DEFAULT_PROMPT = "Build a todo web app with add, complete and delete task features";
const DEFAULT_STACK = "html-css-js";

const INITIAL_STATUSES: Record<AgentRole, StepStatus> = {
  Planner: "Pending",
  Architect: "Pending",
  Developer: "Pending",
  Reviewer: "Pending",
};

function shortId() {
  return Math.random().toString(36).slice(2, 10);
}

function Dashboard() {
  const [prompt, setPrompt] = useState(DEFAULT_PROMPT);
  const [targetStack, setTargetStack] = useState(DEFAULT_STACK);
  const [isGenerating, setIsGenerating] = useState(false);
  const [generation, setGeneration] = useState<Generation | null>(null);
  const [statuses, setStatuses] = useState<Record<AgentRole, StepStatus>>(INITIAL_STATUSES);
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [streams, setStreams] = useState<Record<string, string>>({});
  const [files, setFiles] = useState<GeneratedFile[]>([]);
  const [apiError, setApiError] = useState<string | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const generationIdRef = useRef<string | null>(null);

  const appendLog = useCallback((message: string, level: LogEntry["level"] = "info") => {
    setLogs((prev) => [
      ...prev,
      {
        id: shortId(),
        timestamp: new Date().toLocaleTimeString(),
        message,
        level,
      },
    ]);
  }, []);

  const refreshGeneration = useCallback(async (id: string) => {
    try {
      const final = await api.getGeneration(id);
      setGeneration((g) => ({ ...(g ?? { id, userPrompt: "", targetStack: "" }), ...final }));
      if (final.files?.length) setFiles(final.files.map(normalizeFile));
    } catch {
      /* noop */
    }
  }, []);

  const finalizeGeneration = useCallback(
  async (id: string, status: Generation["status"], error?: string) => {
    setIsGenerating(false);

    try {
      const final = await api.getGeneration(id);
      setGeneration({ ...final, status, error });
      if (final.files?.length) setFiles(final.files.map(normalizeFile));
    } catch {
      setGeneration((g) => (g ? { ...g, status, error } : g));
    } finally {
      setIsGenerating(false);
    }
  },
  [],
);

  const getStep = (p: any) => p?.step ?? p?.Step ?? p?.agent ?? p?.Agent ?? p?.stepName ?? p?.StepName ?? p?.name ?? p?.Name ?? "";
  const getStatus = (p: any) => p?.status ?? p?.Status ?? "";
  const getFile = (p: any) => p?.file ?? p?.File ?? p?.path ?? p?.Path ?? p?.fileName ?? p?.FileName ?? "";
  const getDelta = (p: any) => p?.delta ?? p?.Delta ?? p?.content ?? p?.Content ?? "";
  const getError = (p: any) => p?.error ?? p?.Error ?? p?.message ?? p?.Message ?? "";

  const normalizeFile = (file: any): GeneratedFile => ({
  ...file,
  path: file.path ?? file.relativePath ?? file.RelativePath ?? file.fileName ?? file.FileName ?? "",
  content: file.content ?? file.Content ?? "",
});

  const handleEvent = useCallback(
    (name: RealtimeEventName, payload: any) => {
      const p = payload ?? {};
      switch (name) {
        case "GenerationStarted":
          appendLog("Generation started", "info");
          break;
        case "StepStarted": {
          const agent = getStep(p) as AgentRole;
          if (agent && agent in INITIAL_STATUSES) {
            setStatuses((s) => ({ ...s, [agent]: "Running" }));
          }
          appendLog(`Step started: ${agent || JSON.stringify(p)}`, "agent");
          break;
        }
        case "StepCompleted": {
          const agent = getStep(p) as AgentRole;
          if (agent && agent in INITIAL_STATUSES) {
            setStatuses((s) => ({ ...s, [agent]: "Completed" }));
          }
          appendLog(`Step completed: ${agent || JSON.stringify(p)}`, "success");
          break;
        }
        case "AgentMessage": {
          const agent = p.role ?? p.Role ?? getStep(p) ?? "Agent";
          const msg = p.content ?? p.Content ?? p.message ?? p.Message ?? JSON.stringify(p);
          appendLog(`[${agent}] ${msg}`, "agent");
          break;
        }
        case "FileGenerationStarted": {
          const path = getFile(p);
          if (path) {
            setFiles((f) => (f.find((x) => x.path === path) ? f : [...f, { path, content: "" }]));
          }
          appendLog(`File generation started: ${path}`, "info");
          break;
        }
        case "FileGenerated": {
          const path = getFile(p);
          const content = p.content ?? "";
          if (path) {
            setFiles((f) => {
              const exists = f.find((x) => x.path === path);
              if (exists) {
                return f.map((x) =>
                  x.path === path
                    ? { ...x, path, content: content || x.content }
                    : x,
                );
              }

              return [...f, { path, content }];
            });
            appendLog(`File generated: ${path}`, "success");
            const id = generationIdRef.current;
            if (id) refreshGeneration(id);
          }
          break;
        }
        case "FileContentStreamingStarted": {
          const path = getFile(p);
          if (path) setStreams((s) => ({ ...s, [path]: "" }));
          appendLog(`Streaming started: ${path ?? ""}`, "info");
          break;
        }
        case "FileContentDelta": {
          const path = getFile(p);
          const delta = getDelta(p);
          if (path) {
            setStreams((s) => ({ ...s, [path]: (s[path] ?? "") + delta }));
          }
          break;
        }
        case "FileContentStreamingCompleted": {
          const path = getFile(p);
          appendLog(`Streaming completed: ${path ?? ""}`, "success");
          break;
        }
        case "GenerationCompleted": {
          appendLog("Generation completed", "success");
          const id = generationIdRef.current;
          if (id) finalizeGeneration(id, "Completed");
          else setIsGenerating(false);
          break;
        }
        case "GenerationFailed": {
          const err = getError(p);
          appendLog(`Generation failed: ${err}`, "error");
          const id = generationIdRef.current;
          if (id) finalizeGeneration(id, "Failed", err);
          else setIsGenerating(false);
          break;
        }
      }
    },
    [appendLog, finalizeGeneration, refreshGeneration],
  );

  async function handleGenerate() {
    console.log("Generate clicked");
    setApiError(null);
    setLogs([]);
    setStreams({});
    setFiles([]);
    setStatuses(INITIAL_STATUSES);
    setGeneration(null);
    setIsGenerating(true);

    try {
      if (connectionRef.current) {
        await connectionRef.current.stop().catch(() => {});
        connectionRef.current = null;
      }

      const created = await api.createGeneration({ userPrompt: prompt, targetStack });
      const id = created.id;
      generationIdRef.current = id;
      setGeneration({
        id,
        userPrompt: prompt,
        targetStack,
        status: "Generating",
      });
      appendLog(`Generation created: ${id}`, "info");

      const conn = await createGenerationConnection(id, handleEvent);
      connectionRef.current = conn;
      appendLog("Joined generation hub", "success");

      await api.runGeneration(id);
      appendLog("Run requested", "info");
    } catch (e) {
      const msg = e instanceof Error ? e.message : "Unknown error";
      setApiError(msg);
      appendLog(`Error: ${msg}`, "error");
      setIsGenerating(false);
    }
  }

  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="border-b border-border bg-card/50 backdrop-blur">
        <div className="mx-auto flex max-w-[1600px] items-center justify-between px-6 py-4">
          <div className="flex items-center gap-3">
            <div
              className="flex h-9 w-9 items-center justify-center rounded-xl"
              style={{ background: "var(--gradient-primary)" }}
            >
              <Sparkles className="h-5 w-5 text-primary-foreground" />
            </div>
            <div>
              <h1 className="text-base font-semibold leading-tight">Multi-Agent Coding Assistant</h1>
              <p className="text-xs text-muted-foreground">agent-coding-ui · powered by .NET 9</p>
            </div>
          </div>
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <Activity className="h-4 w-4" />
            <span>
              {generation
                ? `${generation.status}${generation.id ? ` · ${generation.id.slice(0, 8)}` : ""}`
                : "Idle"}
            </span>
          </div>
        </div>
      </header>

      <main className="mx-auto grid max-w-[1600px] gap-5 px-6 py-6 lg:grid-cols-[360px_1fr_340px]">
        <PromptComposer
          prompt={prompt}
          setPrompt={setPrompt}
          targetStack={targetStack}
          setTargetStack={setTargetStack}
          onGenerate={handleGenerate}
          isGenerating={isGenerating}
        />

        <div className="flex flex-col gap-5">
          {apiError && (
            <div className="rounded-xl border border-destructive/60 bg-destructive/10 p-3 text-sm text-destructive">
              {apiError}
            </div>
          )}
          {generation?.status === "Failed" && generation.error && (
            <div className="rounded-xl border border-destructive/60 bg-destructive/10 p-3 text-sm text-destructive">
              Generation failed: {generation.error}
            </div>
          )}
          <AgentTimeline statuses={statuses} />
          <div className="grid gap-5 xl:grid-cols-2">
            <RealtimeLogs logs={logs} />
            <LiveStreamingPreview streams={streams} />
          </div>
          <GeneratedFilesExplorer files={files} />
        </div>

        <ActionsPanel generation={generation} projectName="agent-coding-ui" />
      </main>
    </div>
  );
}
