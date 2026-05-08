import { defineConfig } from "@lovable.dev/vite-tanstack-config";

export default defineConfig({
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "https://localhost:7279",
        changeOrigin: true,
        secure: false,
      },
      "/hubs": {
        target: "https://localhost:7279",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});