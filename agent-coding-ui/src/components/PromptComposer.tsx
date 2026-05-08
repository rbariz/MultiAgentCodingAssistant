import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Sparkles, Loader2 } from "lucide-react";

interface Props {
  prompt: string;
  setPrompt: (v: string) => void;
  targetStack: string;
  setTargetStack: (v: string) => void;
  onGenerate: () => void;
  isGenerating: boolean;
}

export function PromptComposer({
  prompt,
  setPrompt,
  targetStack,
  setTargetStack,
  onGenerate,
  isGenerating,
}: Props) {
  return (
    <div className="flex h-full flex-col gap-5 rounded-2xl border border-border bg-card p-6 shadow-[var(--shadow-glow)]">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Prompt Composer</h2>
        <p className="text-sm text-muted-foreground">Describe the project you want the agents to build.</p>
      </div>
      <div className="flex flex-col gap-2">
        <Label htmlFor="prompt">User prompt</Label>
        <Textarea
          id="prompt"
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          rows={10}
          className="resize-none bg-background/60 font-mono text-sm"
        />
      </div>
      <div className="flex flex-col gap-2">
        <Label htmlFor="stack">Target stack</Label>
        <Input
          id="stack"
          value={targetStack}
          onChange={(e) => setTargetStack(e.target.value)}
          className="bg-background/60 font-mono text-sm"
        />
        <div className="flex flex-wrap gap-2 pt-1">
          {["html-css-js", "react-ts", "next-ts", "vue-ts"].map((s) => (
            <button
              key={s}
              type="button"
              onClick={() => setTargetStack(s)}
              className="rounded-full border border-border bg-secondary px-3 py-1 text-xs text-secondary-foreground transition hover:border-primary"
            >
              {s}
            </button>
          ))}
        </div>
      </div>
      <Button
        onClick={onGenerate}
        disabled={isGenerating || !prompt.trim()}
        size="lg"
        className="mt-auto h-12 w-full text-base font-semibold"
        style={{ background: "var(--gradient-primary)" }}
      >
        {isGenerating ? (
          <>
            <Loader2 className="mr-2 h-5 w-5 animate-spin" /> Generating…
          </>
        ) : (
          <>
            <Sparkles className="mr-2 h-5 w-5" /> Generate Project
          </>
        )}
      </Button>
    </div>
  );
}
