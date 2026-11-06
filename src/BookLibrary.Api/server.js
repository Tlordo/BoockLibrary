import express from "express";
import { DatabaseSync } from "node:sqlite"; // встроенный SQLite в Node.js (22.5+)

const PORT = process.env.PORT || 3000;
const DB_PATH = process.env.DB_PATH || "booklibrary.db";

const db = new DatabaseSync(DB_PATH);
db.exec(`
  CREATE TABLE IF NOT EXISTS factions (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    name        TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT ''
  )
`);

const seed = [
  ["Империум Человечества", "Человечество под властью Императора"],
  ["Космический Десант", "Генетически улучшенные воины Императора"],
  ["Эльдары", "Древняя раса псайкеров"],
  ["Тёмные Эльдары", "Друхари — садисты из Комморры"],
  ["Орки", "ВАААГХ! Зелёная раса, созданная Кроками"],
  ["Тираниды", "Космический рой, пожирающий миры"],
  ["Некроны", "Древние металлические повелители"],
  ["Тау", "Молодая империя Высшего Блага"],
  ["Хаос", "Силы Варпа и демоны"],
  ["Генокрады", "Культы, заражённые тиранидами"],
];
if (db.prepare("SELECT COUNT(*) AS n FROM factions").get().n === 0) {
  const ins = db.prepare("INSERT INTO factions (name, description) VALUES (?, ?)");
  for (const [name, description] of seed) ins.run(name, description);
}

const q = {
  all: db.prepare("SELECT id, name, description FROM factions ORDER BY id"),
  one: db.prepare("SELECT id, name, description FROM factions WHERE id = ?"),
  insert: db.prepare("INSERT INTO factions (name, description) VALUES (?, ?)"),
  update: db.prepare("UPDATE factions SET name = ?, description = ? WHERE id = ?"),
  remove: db.prepare("DELETE FROM factions WHERE id = ?"),
};

const app = express();
app.use(express.json());

const toId = (s) => (/^\d+$/.test(s) ? Number(s) : -1);
const validName =(b) => typeof b?.name === "string" && b.name.trim() !== "";

app.get("/health", (_req, res) => res.json({ status: "ok", service: "BookLibrary.Api" }));

app.get("/factions", (_req, res) => res.json(q.all.all()));

app.get("/factions/:id", (req, res) => {
  const f = q.one.get(toId(req.params.id));
  f ? res.json(f) : res.sendStatus(404);
});

app.post("/factions", (req, res) => {
  if (!validName(req.body)) return res.status(400).json({ error: "Name обязателен" });
  const { lastInsertRowid } = q.insert.run(req.body.name, req.body.description ?? "");
  res.status(201).location(`/factions/${lastInsertRowid}`).json(q.one.get(lastInsertRowid));
});

app.put("/factions/:id", (req, res) => {
  if (!validName(req.body)) return res.status(400).json({ error: "Name обязателен" });
  const id = toId(req.params.id);
  const { changes } = q.update.run(req.body.name, req.body.description ?? "", id);
  changes ? res.json(q.one.get(id)) : res.sendStatus(404);
});

app.delete("/factions/:id", (req, res) => {
  const { changes } = q.remove.run(toId(req.params.id));
  res.sendStatus(changes ? 204 : 404);
});

app.listen(PORT, () => console.log(`BookLibrary.Api (JS) слушает http://localhost:${PORT}`));
