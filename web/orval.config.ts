import { defineConfig } from "orval";

export default defineConfig({
  groceryManager: {
    input: {
      target: "http://localhost:5080/openapi/v1.json",
    },
    output: {
      mode: "tags-split",
      target: "./lib/api/generated/grocery-manager.ts",
      schemas: "./lib/api/generated/models",
      client: "fetch",
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
        },
        mutator: {
          path: "./lib/api/api-client.ts",
          name: "apiFetch",
        },
      },
    },
  },
});
