import test from "node:test";
import assert from "node:assert/strict";
import { AgentPackageManager } from "../src/index.js";

test("adds and returns package metadata", () => {
  const apm = new AgentPackageManager();

  apm.add({
    name: "voluntr-agent",
    version: "1.0.0",
    metadata: { runtime: "node" }
  });

  assert.deepEqual(apm.get("voluntr-agent"), {
    version: "1.0.0",
    metadata: { runtime: "node" }
  });
});

test("returns package list", () => {
  const apm = new AgentPackageManager();

  apm.add({ name: "a" });
  apm.add({ name: "b", version: "2.0.0" });

  assert.equal(apm.list().length, 2);
});
