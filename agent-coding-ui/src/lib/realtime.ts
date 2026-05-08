import * as signalR from "@microsoft/signalr";
import { API_BASE_URL } from "./api";

export type RealtimeEventName =
  | "GenerationStarted"
  | "StepStarted"
  | "StepCompleted"
  | "AgentMessage"
  | "FileGenerationStarted"
  | "FileGenerated"
  | "FileContentStreamingStarted"
  | "FileContentDelta"
  | "FileContentStreamingCompleted"
  | "GenerationCompleted"
  | "GenerationFailed";

export const REALTIME_EVENTS: RealtimeEventName[] = [
  "GenerationStarted",
  "StepStarted",
  "StepCompleted",
  "AgentMessage",
  "FileGenerationStarted",
  "FileGenerated",
  "FileContentStreamingStarted",
  "FileContentDelta",
  "FileContentStreamingCompleted",
  "GenerationCompleted",
  "GenerationFailed",
];

export async function createGenerationConnection(
  generationId: string,
  onEvent: (name: RealtimeEventName, payload: any) => void,
): Promise<signalR.HubConnection> {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/generations`)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  for (const evt of REALTIME_EVENTS) {
    connection.on(evt, (payload: any) => onEvent(evt, payload));
  }

  await connection.start();
  await connection.invoke("JoinGeneration", generationId);
  return connection;
}
