# Checklist

The app is running at {{BASE_URL}}. Sign in as a staff member by opening {{BASE_URL}}/dev-login?name=sam&roles=Staff, then open {{BASE_URL}}/admin. Find your way around the admin area as a user would, through its links and controls; don't guess URLs unless a check needs one.

The database was freshly seeded: 4 customers (Ana Ribeiro, Lena Fischer, Tomás García, Priya Nair) and 24 bookings, VT-1000 to VT-1023. Each customer has 6 bookings, and each status (Pending, Confirmed, Cancelled) has 8. Departure dates rise with the reference number, and so do totals (VT-1000 is 450.00, VT-1023 is 3,325.00).

Grade each check pass, partial or fail. Pass means it works fully as described. Partial means it works in part (for example search by reference but not by customer name). Fail means it's missing or broken. Note what you saw in a sentence. Do the checks in order: the later ones change the data.

## Lists

- L1: A customers list shows the 4 seeded customers.
- L2: The bookings list shows reference, customer name, destination, departure date, status and total, with the total formatted as currency.
- L3: The bookings list shows 10 bookings per page, and the remaining bookings can be reached (3 pages in all).
- L4: Searching the bookings for `VT-1005` shows only that booking.
- L5: Searching the bookings for `Priya` shows only Priya Nair's 6 bookings.
- L6: Filtering the bookings by status Cancelled shows only cancelled bookings (8), and the filter can be cleared to show all 24 again.
- L7: The bookings can be sorted by departure date, in both directions.
- L8: The bookings can be sorted by total, in both directions.

## Forms

- F1: Creating a booking works: on the form, the customer is chosen from a searchable list (type part of a name to find them), not typed as an ID. Fill in the rest with sensible values, save, and find the new booking in the list.
- F2: Validation: submitting the booking form with the destination empty, and again with 13 travellers, shows a validation message each time and saves nothing.
- F3: Editing a booking works: change VT-1003's destination to Nairobi, save, and see the change in the list.
- F4: Deleting a booking works: delete VT-1004 and check it's gone from the list.
- F5: Customers can be created, edited and deleted.
