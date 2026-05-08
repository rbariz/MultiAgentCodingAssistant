import type { CreateGenerationRequest, Generation } from "@/types/generation";
import type { GitHubPublishRequest, GitHubPublishResponse } from "@/types/github";

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;

async function jsonFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
  });
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`${res.status} ${res.statusText}: ${text}`);
  }
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export const api = {
  createGeneration: (body: CreateGenerationRequest) =>
    jsonFetch<Generation & { id: string }>("/api/generations", {
      method: "POST",
      body: JSON.stringify(body),
    }),
  runGeneration: (id: string) =>
    jsonFetch<void>(`/api/generations/${id}/run`, { method: "POST" }),
  getGeneration: (id: string) => jsonFetch<Generation>(`/api/generations/${id}`),
  exportUrl: (id: string) => `${API_BASE_URL}/api/generations/${id}/export`,
  publishGitHub: (body: GitHubPublishRequest) =>
    jsonFetch<GitHubPublishResponse>("/api/github/publish", {
      method: "POST",
      body: JSON.stringify(body),
    }),
};
