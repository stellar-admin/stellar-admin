Add an admin area to this app for Voyager's staff, at `/admin`.

- Staff can list, create, edit and delete customers and bookings.
- The bookings list shows each booking's reference, customer name, destination, departure date, status and total (as currency). Staff can search it by reference or customer name, filter it by status, and sort it by departure date and by total. It shows 10 bookings per page.
- On the booking form, staff choose the customer from a searchable list instead of typing an ID. Invalid input, such as a missing destination or more than 12 travellers, shows validation messages instead of saving.
- Only signed-in users with the Staff role may use the admin area; everyone else is refused.

Make sure it builds and works.
