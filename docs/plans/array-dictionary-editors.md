# Array and dictionary editors

Status: proposed (2026-10-23). Not planned in detail; the agent who picks it up decides the design and writes the plan for Jerrie's approval.

## Idea

Built-in admin panel editors for quickly entering a list of items (array editor) or a set of key-value pairs (dictionary editor).

## Context

- Raised by Jerrie during [foreign-key editors](archive/foreign-key-editors.md) (Stage C of [configuration simplification](archive/configuration-simplification.md)).
- An array editor might also become the default editor for an EF Core many-to-many field whose related type isn't a registered resource. Today such a field is left out of the form with a warning.
