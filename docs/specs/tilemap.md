# Tilemap Save Format & Type Registry — Implementation Spec

## Purpose

Defines (1) the JSON save format for the 3D tile editor's world files and
(2) the type registry that maps save-file type keys to game classes. This spec
is the source of truth for the loader, the editor's save routine, and the type
factory. Implement all three so they agree exactly.

Out of scope: the editor UI, the runtime, and the map/world data structures.
This document specifies the on-disk format and the type-resolution layer only.

---

## 1. Concepts and invariants

These hold across the whole system; violating one is a bug even if the JSON
parses.

- **Tile footprint size lives in the class, never in the file.** Each tile
  class has a hardcoded `TileSize` (one of `OneByOne`, `OneByTwo`,
  `OneByThree`, `TwoByOne`, `TwoByTwo`, `TwoByThree`, `ThreeByOne`,
  `ThreeByTwo`, `ThreeByThree`, expressed as `W x D` cells). The loader looks
  up size from the resolved class; it is never read from JSON.
- **Colliders are derived, never stored.** Collider size/offset come from the
  class plus the rotation step. Not in the file.
- **Grid tiles vs. free objects are separate.** Grid tiles are cell-aligned,
  integer-positioned, quarter-turn rotated, and stored per layer. Free objects
  (decorations) have float positions, arbitrary Y-yaw, optional scale, and are
  stored in one flat list.
- **Type keys are stable strings decoupled from class names.** A class rename
  must not break saved files. Keys are declared explicitly at registration
  (Section 4), never derived from `nameof`/reflection on the class name.
- **The file is self-contained and rewritten atomically on save.** The whole
  file is regenerated each save, so the palette may be compacted/renumbered
  safely (all indices are rewritten together).

---

## 2. JSON format

### 2.1 Top-level example (canonical)

```json
{
  "format": "tilemap",
  "version": 1,
  "name": "Forest Level 1",
  "grid": {
    "width": 64,
    "depth": 64,
    "cellSize": 1.0,
    "layerHeight": 1.0,
    "layerCount": 5
  },
  "palette": ["GrassTile", "WaterTile", "StoneWall", "Door"],
  "layers": [
    {
      "index": 0,
      "role": "ground",
      "tiles": [
        { "t": 0, "x": 0, "z": 0, "r": 0 },
        { "t": 1, "x": 5, "z": 3, "r": 2 }
      ]
    },
    {
      "index": 1,
      "tiles": [
        { "t": 2, "x": 4, "z": 2, "r": 1 },
        {
          "t": 3,
          "x": 6,
          "z": 2,
          "r": 1,
          "properties": { "locked": true, "keyId": "bronze" }
        }
      ]
    }
  ],
  "objects": [
    {
      "type": "Torch",
      "pos": [3.5, 1.0, 2.25],
      "rot": [0, 90, 0],
      "properties": { "lit": true }
    },
    { "type": "Barrel", "pos": [8.0, 1.0, 4.0], "rot": [0, 0, 0] }
  ]
}
```

### 2.2 Root object

| Field     | Type     | Required | Notes                                                         |
| --------- | -------- | -------- | ------------------------------------------------------------- |
| `format`  | string   | yes      | Must equal `"tilemap"`. Loader rejects otherwise.             |
| `version` | int      | yes      | Currently `1`. See Section 5 for migration policy.            |
| `name`    | string   | no       | Human-readable map name.                                      |
| `grid`    | object   | yes      | See 2.3.                                                      |
| `palette` | string[] | yes      | Tile type keys; index into this is the `t` on tiles. See 2.6. |
| `layers`  | object[] | yes      | See 2.4.                                                      |
| `objects` | object[] | no       | Free decorations. See 2.5. Absent = empty.                    |

### 2.3 `grid`

| Field         | Type  | Required | Notes                                              |
| ------------- | ----- | -------- | -------------------------------------------------- |
| `width`       | int   | yes      | Cell count along X.                                |
| `depth`       | int   | yes      | Cell count along Z.                                |
| `cellSize`    | float | yes      | World units per cell edge.                         |
| `layerHeight` | float | yes      | World units between consecutive layers along Y.    |
| `layerCount`  | int   | yes      | Number of valid layer indices (0 .. layerCount-1). |

World Y of a grid tile on layer `L` is `L * layerHeight`. Height is **never**
stored on a tile record — it is implied by the layer.

### 2.4 `layers[]`

| Field   | Type     | Required | Notes                                                                       |
| ------- | -------- | -------- | --------------------------------------------------------------------------- |
| `index` | int      | yes      | Layer index, `0 .. layerCount-1`. Determines Y.                             |
| `role`  | string   | no       | Semantic tag. Layer 0 uses `"ground"` or `"water"`. Omit for height layers. |
| `tiles` | object[] | yes      | Tile records. May be empty.                                                 |

Layer 0 is the ground/water plane; layers 1..N are height levels. Do not assume
layers are ordered or contiguous in the array — key off `index`.

### 2.5 Records

**Grid tile** (inside `layers[].tiles`):

| Field        | Type   | Required | Notes                                                                 |
| ------------ | ------ | -------- | --------------------------------------------------------------------- |
| `t`          | int    | yes      | Index into root `palette`.                                            |
| `x`          | int    | yes      | Anchor cell X. Anchor = footprint min-corner at `r == 0` (Section 3). |
| `z`          | int    | yes      | Anchor cell Z.                                                        |
| `r`          | int    | yes      | Rotation step: `0,1,2,3` = 0/90/180/270 deg yaw about Y.              |
| `properties` | object | no       | Free-form per-instance data passed to the factory (Section 3.4).      |

**Free object** (inside root `objects`):

| Field        | Type     | Required | Notes                                                                                     |
| ------------ | -------- | -------- | ----------------------------------------------------------------------------------------- |
| `type`       | string   | yes      | Type key, resolved directly through the object registry (Section 4). Not a palette index. |
| `pos`        | float[3] | yes      | World position `[x, y, z]`.                                                               |
| `rot`        | float[3] | yes      | Euler degrees `[x, y, z]`. In practice only Y is nonzero, but store all three.            |
| `scale`      | float[3] | no       | Defaults to `[1,1,1]` if absent.                                                          |
| `properties` | object   | no       | Free-form per-instance data passed to the factory.                                        |

Note the deliberate asymmetry: tiles reference the palette by integer `t`;
objects carry their `type` string inline (objects are few and this keeps them
readable). Both resolve through the **same** registry (Section 4).

### 2.6 Palette semantics

- `palette` is an array of tile type keys, unique, order = discovery order (the
  order tiles were first placed in the editor).
- A tile's `t` is an index into this array. Indices are **per-map**; never
  hardcode a numeric index anywhere. Always resolve `palette[t]` then the
  registry.
- The palette grows as new tile types are placed: on placing a type not already
  present, append it and use the new index (string-interning pattern).
- On save, the editor **may** compact the palette (drop entries no longer used
  by any tile) and renumber, because every `t` is rewritten in the same pass.
  Compaction is optional; an unused entry is harmless.
- `palette` covers **tiles only**. Objects do not appear in it.

---

## 3. Placement, rotation, and occupancy

### 3.1 Rotation

- Rotation is yaw about the world **Y axis** only, in quarter turns.
  `r` in `{0,1,2,3}` maps to `r * 90` degrees.
- The visual mesh and the collider rotate about the tile's **true center**
  (bottom-center pivot in the horizontal plane). The existing
  `Tile3D.RotateTile` already implements this: it snaps to the nearest 90-deg
  step and, on odd steps, swaps the collider AABB's X/Z extents. The loader
  sets `r` by calling this method; it does not write pivot data.

### 3.2 Footprint under rotation

A footprint is `W x D` cells (X by Z) at `r == 0`. A quarter turn swaps the
axes:

- `r` even (0, 2): occupied extent is `W` along X, `D` along Z.
- `r` odd (1, 3): occupied extent is `D` along X, `W` along Z.

### 3.3 Occupancy rule (loader and editor MUST match)

The anchor `(x, z)` is the footprint's min-corner at `r == 0`. To compute
occupied cells at any `r`, hold the tile's center fixed and re-derive the
min-corner, then round to the nearest cell:

```
centerX = x + W / 2.0
centerZ = z + D / 2.0

if r is even:  extX = W,  extZ = D
else:          extX = D,  extZ = W

minX = round(centerX - extX / 2.0)
minZ = round(centerZ - extZ / 2.0)

occupied cells = { (cx, cz) : minX <= cx < minX + extX,
                              minZ <= cz < minZ + extZ }
```

Use the same rounding (round-half-up, or round-half-to-even — pick one and use
it in BOTH the editor and the loader; they must produce identical cell sets).

**Parity note.** When `W` and `D` share parity (both odd or both even), the
center-minus-extent term is already integer and the round is exact:
`1x1, 2x2, 3x3, 1x3, 3x1`. When parity is mixed, a quarter turn lands the true
center a half-cell off the occupancy box; rounding resolves it. The four
mixed-parity sizes are: **`OneByTwo (1x2)`, `TwoByOne (2x1)`, `TwoByThree (2x3)`,
`ThreeByTwo (3x2)`**. For these, the visual mesh center may sit half a cell from
the occupancy box center — this is expected and acceptable. Occupancy stays
integer and on-grid in all cases.

### 3.4 `properties`

`properties` is an optional free-form object carrying per-instance data (a
locked door, a torch's intensity, etc.). It is **not** auto-assigned by the
loader. Instead the loader passes it into the type's factory delegate, and the
constructed object decides what to do with it — reading whatever keys it cares
about and ignoring the rest.

This puts interpretation inside each type rather than in a generic
reflection/assignment pass in the loader. The loader never inspects the keys; it
only forwards the object. See Section 4 for the factory signature.

`properties` may be absent (types with no per-instance data omit it); the loader
passes an empty/null properties object in that case, and every factory must
tolerate that. No fixed schema — each type owns its own keys.

---

## 4. Type registry (the change to implement)

### 4.1 Goal

Two separate registries — one for tiles, one for objects — each resolving a
stable string key to an instance of its own base type. Tiles reach the **tile
registry** via `palette[t]`; objects reach the **object registry** via their
inline `type`. Keys are declared explicitly at registration, so a class rename
touches only its one `Register` line and never breaks save files.

No attributes and no reflection scan. Each key/class mapping is written by hand
in one central place, giving full control over what is registered and where.

Why two registries rather than one:

- **Type safety.** The tile registry hands back a `Tile3D`, so the loader needs
  no cast before calling `RotateTile`; the object registry hands back the object
  base type. A wrong-category class can't be registered on the wrong side.
- **Independent key namespaces.** A tile key and an object key never collide, so
  the two lists evolve without coordination. (Using distinct names anyway is
  still good practice for legibility, but nothing enforces it across registries.)
- **Clear separation of concerns.** The palette (tiles) and the object list read
  and resolve through their own factory, matching how the JSON already splits
  them.

### 4.2 Registration

A small generic registry type serves both, parameterized by the base type it
produces. Each factory delegate takes the record's `properties` so the
constructed object can consume them itself:

```csharp
// The parsed "properties" object from a record. Null/empty when the record
// omits "properties". Bind to your JSON type of choice (e.g. JsonObject or
// IReadOnlyDictionary<string, JsonElement>); factories must tolerate null.
using Properties = System.Text.Json.Nodes.JsonObject;

public class TypeRegistry<T> where T : GameObject
{
    readonly Dictionary<string, Func<Properties, T>> factories = new();

    public void Register(string key, Func<Properties, T> factory)
    {
        if (!factories.TryAdd(key, factory))
            throw new InvalidOperationException($"Duplicate key: '{key}'");
    }

    public T Create(string key, Properties properties) =>
        factories.TryGetValue(key, out var f)
            ? f(properties)
            : throw new KeyNotFoundException($"Unknown type key: '{key}'");

    public bool Contains(string key) => factories.ContainsKey(key);

    public IEnumerable<string> Keys => factories.Keys; // for the editor palette UI
}
```

Two instances, populated explicitly at startup. Each factory receives the
`properties` object and decides what to read from it; a type with no
per-instance data simply ignores the argument:

```csharp
public static class Registries
{
    public static readonly TypeRegistry<Tile3D>    Tiles   = new();
    public static readonly TypeRegistry<GameObject> Objects = new();

    public static void RegisterAll()
    {
        // Tiles — key is what lands in the save file's palette.
        Tiles.Register("GrassTile", props => new GrassTile());
        Tiles.Register("WaterTile", props => new WaterTile());
        Tiles.Register("StoneWall", props => new BrickWall()); // key stable; class renamable
        Tiles.Register("Door",      props => new Door(props));  // Door reads "locked", "keyId"

        // Objects — key is the object record's "type".
        Objects.Register("Torch",  props => new Torch(props));  // Torch reads "lit", "intensity"
        Objects.Register("Barrel", props => new Barrel());
    }
}
```

The factory is where each type interprets its own `properties`. How it does so —
constructor argument (as above), an `Init(props)` method, or ignoring them — is
that type's business; the registry and loader don't care. The loader passes the
same `properties` object into `Create` and does nothing else with it.

Notes:

- The key is the stable identifier that appears in save files (`palette` entries
  for tiles, `type` for objects). The class on the right may be renamed freely;
  only its `Register` line changes.
- `TypeRegistry<Tile3D>` returns `Tile3D`, so the tile loader needs no cast.
  `TypeRegistry<GameObject>` returns the object base.
- Every factory must tolerate a null/empty `properties` argument (records that
  omit the field).
- Keys must be unique **within** each registry; `Register` throws on a duplicate.
  The same string may appear in both registries without conflict, since they are
  separate tables.
- Each registry's `Keys` is the source of truth for what that side can place —
  convenient for building the editor's tile palette and object palette UIs
  independently.

### 4.3 Resolution

- **Tile:** `key = palette[t]` → `Registries.Tiles.Create(key, properties)`
  (returns `Tile3D`, no cast; the tile consumes `properties` internally) → set
  grid transform from `(x, z, layer.index)` → `RotateTile(r * 90deg)`.
- **Object:** `key = record.type` →
  `Registries.Objects.Create(key, properties)` (the object consumes `properties`
  internally) → set transform from `pos`/`rot`/`scale`.
- The loader passes `properties` into `Create` and does not otherwise read or
  assign it. Transform is still set by the loader after construction.
- Unknown key in the relevant registry: fail with a clear error naming the
  missing key, which registry was searched, and where it appeared (palette index,
  or object record). Do not silently drop.

---

## 5. Versioning

- `version` is currently `1`. Bump it on any breaking format change.
- The loader must check `format == "tilemap"` and handle `version`. For an
  unknown higher version, fail with a clear message rather than mis-parsing.
- Because saves are atomic full rewrites, a load-then-save cycle may upgrade an
  older file to the current version.

---

## 6. Loader acceptance checklist

- [ ] Rejects files where `format != "tilemap"` or `version` is unsupported.
- [ ] Reads grid, layers (by `index`, not array order), tiles, and objects.
- [ ] Resolves tiles via `palette[t]` through the tile registry, and objects
      via `type` through the object registry; unknown keys fail loudly naming
      which registry was searched.
- [ ] Computes tile Y as `layer.index * grid.layerHeight`; never reads Y from a
      tile record.
- [ ] Computes occupancy with the Section 3.3 formula and the SAME rounding as
      the editor; verified identical on all four mixed-parity sizes at all four
      `r` values.
- [ ] Calls `RotateTile` for tile rotation; does not store/read pivot data.
- [ ] Passes each record's `properties` into the factory (`Create(key, properties)`)
      for both tiles and objects; the loader does not read or assign `properties`
      itself. Factories tolerate absent/null `properties`.
- [ ] Never reads tile size or collider data from JSON (both come from the class).
- [ ] Round-trip: load then save reproduces an equivalent file (palette may be
      compacted/renumbered; tile/object sets and transforms unchanged).
