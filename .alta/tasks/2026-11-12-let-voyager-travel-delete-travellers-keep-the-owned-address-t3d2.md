---
title: "Let Voyager Travel delete travellers: keep the owned Address's ownership key out of the Restrict loop"
kind: problem
status: later
created: 2026-11-12
summary: "ApplicationDbContext sets every required foreign key to Restrict, including Traveller.Address's ownership key, so EF refuses to delete any traveller. Fix it in the sample app generator and sync tests/VoyagerTravel."
---

## Why

In the Phase 3 agent-eval check, all 9 generated admin apps (StellarAdmin and plain Razor Pages alike) failed to delete a newly created traveller with no bookings. Each app's log shows `InvalidOperationException: The association between entity types 'Traveller' and 'Traveller.Address#Address' has been severed, but the relationship is either marked as required...`.

The cause is the loop in `tests/VoyagerTravel/Data/ApplicationDbContext.cs` (lines 76–83). It sets every required foreign key in the `VoyagerTravel.Models` namespace to `DeleteBehavior.Restrict`. That includes the ownership key of the `[Owned]` `Address`, which has to cascade.

## What to do

Skip ownership keys in the loop, for example `.Where(foreignKey => !foreignKey.IsOwnership)`. Make the change in the sample app generator, the source of the synced model, then sync `tests/VoyagerTravel`.

## Where

- `tests/VoyagerTravel/Data/ApplicationDbContext.cs`
- the sample app generator's copy of the same file
