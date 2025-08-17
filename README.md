# RedisKit for .NET Core

A simple and production-ready **Redis repository wrapper** built on top of [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).  
Supports common operations like **Insert (Set)**, **Get**, **Exists**, **Delete**, and a safe-ish **Update with lock**.

---

## 🚀 Features

- Insert / Update / Delete / Exists
- Optional TTL (expiry) for keys
- JSON serialization via `System.Text.Json`
- Thread-safe, async/await support
- Distributed lock for atomic updates (basic, best-effort)
- Easy **Dependency Injection** with `.AddRedisKit(...)`

---

## 📦 Installation

Add the NuGet dependency:

```bash
dotnet add package StackExchange.Redis
