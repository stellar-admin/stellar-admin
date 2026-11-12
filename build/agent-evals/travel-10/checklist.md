# Checklist

The app is running at {{BASE_URL}}. Sign in as a staff member by opening {{BASE_URL}}/dev-login?name=sam&roles=Staff, then open {{BASE_URL}}/admin. Find your way around the admin area as a user would, through its links and controls; don't guess URLs unless a check needs one.

The database was freshly seeded. Facts for the checks: 70 trips; 1,401 departures; 3,000 travellers; 7,162 bookings, references VB-100001 to VB-107162, with statuses Pending 545, Confirmed 1,173, Cancelled 722 and Completed 4,722. Booking totals range from 760.00 (VB-106674 is one of the lowest) to 28,760.00 (VB-100531 is one of the highest). Only one traveller has the email eda_treutel@hotmail.com (Eda Treutel); 4 travellers have the last name Treutel. Every trip has departures, so deleting a trip may rightly be refused. There are 25 guides, 44 countries and 135 cities; payments have the statuses Succeeded 12,076, Refunded 722 and Failed 271.

Grade each check pass, partial or fail. Pass means it works fully as described. Partial means it works in part (for example, the search finds the record but also unrelated ones). Fail means it's missing or broken. Note what you saw in a sentence. Do the checks in order: the later ones change the data.

## Lists

- L1: Lists exist for trips, departures, travellers, bookings, countries, cities, destinations, guides, payments and booking passengers, each reachable from the admin area's navigation. Each shows 25 records per page with a way to reach the rest.
- L2: Searching the trips for `Kyoto` shows only the two Kyoto trips, VT-JP-01 and VT-JP-02.
- L3: Searching the bookings for `VB-101459` shows only that booking.
- L4: Searching the travellers for `eda_treutel@hotmail.com` shows only Eda Treutel.
- L5: Filtering the bookings by status Cancelled shows 722 bookings, and the filter can be cleared to show all 7,162 again.
- L6: The bookings can be sorted by total in both directions: highest first starts at 28,760.00, lowest first at 760.00, shown as currency.
- L7: The bookings list shows each booking's traveller and departure (or its trip) by name or date, not by ID.
- L8: Searching the guides for `Okuneva` shows only Koby Okuneva.
- L9: Searching the cities for `Mendoza` shows only Mendoza, with its country (Argentina) by name.
- L10: Filtering the payments by status Refunded shows 722 payments.

## Forms

- F1: Creating a booking works: its traveller and departure are chosen from searchable lists (type `Treutel` to find Eda Treutel), not typed as IDs. Fill in the rest with sensible values, save, and find the new booking in the list.
- F2: Validation: saving trip VT-IT-04 ("Rome in Depth") with its title emptied shows a validation message and saves nothing.
- F3: Editing works: change the title of trip VT-IT-04 to "Rome in Depth and Detail", save, and see the change in the trips list.
- F4: Deleting a booking works: delete VB-101459 and check it's gone.
- F5: Travellers can be created, edited and deleted.
- F6: Creating a city works: its country is chosen from a searchable list. Then edit the new city's name, and delete it.
