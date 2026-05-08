import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Download, Github, ExternalLink, Loader2 } from "lucide-react";
import { api, API_BASE_URL } from "@/lib/api";
import type { Generation } from "@/types/generation";

interface Props {
  generation: Generation | null;
  projectName: string;
}

export function ActionsPanel({ generation, projectName }: Props) {
  const [token, setToken] = useState("");
  const [repoName, setRepoName] = useState(projectName);
  const [isPrivate, setIsPrivate] = useState(true);
  const [publishing, setPublishing] = useState(false);
  const [repoUrl, setRepoUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const completed = generation?.status === "Completed";
  const exportHref = generation ? `${API_BASE_URL}/api/generations/${generation.id}/export` : "#";

  async function handlePublish() {
    if (!generation) return;
    setError(null);
    setRepoUrl(null);
    setPublishing(true);
    try {
      const res = await api.publishGitHub({
        generationId: generation.id,
        repositoryName: repoName,
        isPrivate,
        githubToken: token,
      });
      setRepoUrl(res.repositoryUrl || res.htmlUrl || res.url || null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to publish");
    } finally {
      setPublishing(false);
    }
  }

  return (
    <div className="flex h-full flex-col gap-5 rounded-2xl border border-border bg-card p-6">
      <div>
        <h3 className="text-lg font-semibold text-foreground">Actions</h3>
        <p className="text-sm text-muted-foreground">Available after generation completes.</p>
      </div>

      <Button asChild disabled={!completed} variant="secondary" className="w-full" >
        <a
          href={completed ? exportHref : undefined}
          aria-disabled={!completed}
          className={!completed ? "pointer-events-none opacity-50" : ""}
        >
          <Download className="mr-2 h-4 w-4" /> Download ZIP
        </a>
      </Button>

      <div className="border-t border-border pt-5">
        <div className="mb-3 flex items-center gap-2">
          <Github className="h-4 w-4" />
          <h4 className="text-sm font-semibold">Publish to GitHub</h4>
        </div>
        <div className="flex flex-col gap-3">
          <div>
            <Label htmlFor="ghtoken" className="text-xs">GitHub Token</Label>
            <Input
              id="ghtoken"
              type="password"
              value={token}
              onChange={(e) => setToken(e.target.value)}
              placeholder="ghp_..."
              className="bg-background/60"
            />
          </div>
          <div>
            <Label htmlFor="reponame" className="text-xs">Repository name</Label>
            <Input
              id="reponame"
              value={repoName}
              onChange={(e) => setRepoName(e.target.value)}
              className="bg-background/60"
            />
          </div>
          <div className="flex items-center justify-between rounded-md border border-border bg-background/40 px-3 py-2">
            <Label htmlFor="priv" className="text-xs">Private repository</Label>
            <Switch id="priv" checked={isPrivate} onCheckedChange={setIsPrivate} />
          </div>
          <Button
            onClick={handlePublish}
            disabled={!completed || publishing || !token || !repoName}
            className="w-full"
            style={{ background: "var(--gradient-primary)" }}
          >
            {publishing ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Github className="mr-2 h-4 w-4" />}
            Publish
          </Button>
          {repoUrl && (
            <a
              href={repoUrl}
              target="_blank"
              rel="noreferrer"
              className="flex items-center gap-1 text-xs text-primary hover:underline"
            >
              Open repository <ExternalLink className="h-3 w-3" />
            </a>
          )}
          {error && <div className="text-xs text-destructive">{error}</div>}
        </div>
      </div>
    </div>
  );
}
