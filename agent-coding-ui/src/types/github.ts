export interface GitHubPublishRequest {
  generationId: string;
  repositoryName: string;
  isPrivate: boolean;
  githubToken: string;
}

export interface GitHubPublishResponse {
  repositoryUrl?: string;
  url?: string;
  htmlUrl?: string;
  error?: string;
}
