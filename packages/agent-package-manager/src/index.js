export class AgentPackageManager {
  #packages = new Map();

  add(pkg) {
    if (!pkg || typeof pkg.name !== "string" || pkg.name.length === 0) {
      throw new Error("Package name is required");
    }

    this.#packages.set(pkg.name, {
      version: pkg.version ?? "0.0.0",
      metadata: pkg.metadata ?? {}
    });
  }

  get(name) {
    return this.#packages.get(name) ?? null;
  }

  list() {
    return [...this.#packages.entries()].map(([name, data]) => ({
      name,
      ...data
    }));
  }
}
