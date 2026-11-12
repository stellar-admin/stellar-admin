Add an admin area to this app for Voyager Travel's staff, at `/admin`, covering all of the app's records: accommodations, amenities, booking add-ons, booking passengers, bookings, cities, countries, departures, destinations, excursions, guides, itinerary days, languages, payments, reviews, room allocations, suppliers, supplier invoices, travellers, trips and trip tags.

For each kind of record, staff can:

- List the records, 25 per page, with a way to reach the rest. Lists show related records by name rather than ID, and amounts as currency.
- Search a list by its main text (such as names, titles, codes, references and emails), filter it by status where the record has one, and sort it by its columns.
- Create, edit and delete records. Invalid input shows validation messages instead of saving.
- Choose related records (such as a booking's traveller and departure) from a searchable list instead of typing an ID.

Only signed-in users with the Staff role may use the admin area; everyone else is refused.

Make sure it builds and works.
