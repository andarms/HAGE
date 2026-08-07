# Hamaze coding conventions

## Comments

Don't add comments. Code should be self-explanatory through clear naming.

- If a method or block of logic is complex, extract it into a well-named method instead of
  explaining it with a comment. The method name should say what it does.
- Variable and method names should be specific enough that a reader never needs a comment to
  understand intent.
- Only exception: a short comment for a truly non-obvious reason — a hidden constraint, a bug
  workaround, a subtle invariant — never to describe what the code does.
- Any comment or doc text that is written must follow ASD-STE100 (Simplified Technical English):
  short sentences, one instruction or idea per sentence, active voice, approved simple
  vocabulary, no jargon or strung-together clauses.

## C# style

- No `_` prefix on fields.
- No `private` keyword — it's the default, don't write it.
- No expression-bodied members (`=>` one-liners), except for getters and simple properties.
- No one-line `if`/`for`/`while`/`foreach` statements — always use `{ }` blocks, even for a
  single statement.
