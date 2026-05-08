export type AgentRole = "Planner" | "Architect" | "Developer" | "Reviewer";
export type StepStatus = "Pending" | "Running" | "Completed" | "Failed";

export interface GeneratedFile {
  path: string;
  content?: string;
}

export interface Generation {
  id: string;
  userPrompt: string;
  targetStack: string;
  status: "Pending" | "Running" | "Completed" | "Failed";
  files?: GeneratedFile[];
  error?: string;
}

export interface CreateGenerationRequest {
  userPrompt: string;
  targetStack: string;
}

export interface LogEntry {
  id: string;
  timestamp: string;
  message: string;
  level: "info" | "success" | "error" | "agent";
}
