ABC PHARMACY  CORE LOGIC AND USER GUIDE
=========================================


1. WHAT THE APPLICATION DOES
----------------------------
ABC Pharmacy keeps track of the medicines in stock and the sales made from that stock.

   Backend : .NET 10 Web API (backend/AbcPharmacy.Api)
   Frontend: Angular 22 with Angular Material (frontend)
   Storage : two JSON files on the server, medicines.json and sales.json


2. CORE LOGIC
-------------

2.1 Medicine data
    Every medicine has:
      Full Name, Brand, Expiry Date, Quantity, Price (2 decimals), Notes

    The same medicine with a different expiry date is treated as a separate
    batch. Name + Brand + Expiry Date must be unique.

2.2 Where the data is stored
     JsonDataStore (backend/AbcPharmacy.Api/Data) is a singleton.
     On startup it reads App_Data/medicines.json and App_Data/sales.json into
      memory.
     If the files don't exist (first run), it creates 1000 sample medicines
      and 250 sample sales.
     Every read and write goes through one lock. After every change both files
      are saved again.
     Saving writes a .tmp file first and then replaces the real file, so a
      crash during saving cannot corrupt the data.

2.3 Colour rules in the grid
    The API calculates these flags for each medicine (MedicineService.ToDto):

      IsExpiringSoon = expiry date is earlier than today + 30 days
                       (this includes medicines that are already expired)
      IsLowStock     = quantity is less than 10
      IsExpired      = expiry date is earlier than today

    The grid uses the flags to colour the rows:

      RED    background -> IsExpiringSoon
      YELLOW background -> IsLowStock
      If both are true, the row is RED.

    The 30 days and 10 units come from appsettings.json:

      "Inventory": { "ExpiryWarningDays": 30, "LowStockThreshold": 10 }

2.4 Adding a medicine
    Validation:
       Full Name : required, max 200 characters
       Brand     : required, max 100 characters
       Expiry    : required, cannot be in the past
       Quantity  : required, whole number from 0 to 100000
       Price     : required, 0.01 to 1000000, max 2 decimal places
                    (MaxDecimalPlaces attribute)
       Notes     : optional, max 1000 characters

    If the same Name + Brand + Expiry already exists, the API returns 400 with
    a message.

2.5 Recording a sale (SaleService.AddSale)
    Everything below runs inside the same lock, so two people selling at the
    same time can never sell more than what is in stock.

      1. Find the medicine. If it is not found, the API returns 404.
      2. If the medicine is expired, the API returns 400 ("... is expired and
         cannot be sold").
      3. If the quantity asked for is more than the stock, the API returns 400
         ("Only X left in stock ...").
      4. Reduce the medicine quantity.
      5. Save a sale record with the medicine name, unit price, quantity, total
         and date. The name and price are copied, so the history stays the same
         even if the medicine changes later.

2.6 Search, filter, sort and paging
    GET /api/medicines supports:
      search    part of the medicine name, not case sensitive
      filter    all | expiring | lowstock
      sortBy    fullName | brand | expiryDate | quantity | price
      sortDir   asc | desc
      page      page number, starting at 1
      pageSize  1 to 500

    All of this is done on the server, so only the current page is sent to the
    browser.


3. HOW TO RUN
-------------

Prerequisites
   .NET SDK 10
   Node.js 24 (or 22.22.3 and later). Check with: node -v

Step 1  Start the API (terminal 1)
    cd backend/AbcPharmacy.Api
    dotnet run

    The API runs on http://localhost:5207.
    Test it in the browser: http://localhost:5207/api/medicines

Step 2  Start the frontend (terminal 2)
    cd frontend
    npm install
    npm start

    Open http://localhost:4200 in the browser.

Settings worth knowing
   Frontend API url : frontend/src/environments/environment.development.ts
   Allowed origins  : "AllowedOrigins" in backend/AbcPharmacy.Api/appsettings.json
    (must contain http://localhost:4200)
   Reset the data   : stop the API, delete backend/AbcPharmacy.Api/App_Data,
    then start the API again. The sample data is created again.


4. HOW TO USE THE APPLICATION
-----------------------------
The top menu has two items: Medicines and Sales.
On a phone the menu becomes the button in the top right corner.

4.1 Medicines page
     The grid shows Name, Brand, Expiry Date, Quantity and Price.
     Row colours:
        red    = expires in less than 30 days (or already expired, also marked
                 with an "Expired" tag)
        yellow = less than 10 in stock
     Search : type in "Search by medicine name". The list updates when you
               stop typing.
     Filter : use the All / Expiring in 30 days / Low stock buttons.
     Sort   : click a column header. Click again to reverse the order.
     Paging : use the arrows at the bottom of the grid. "Items per page" lets
               you choose 10, 50, 100, 200 or 500 rows.

4.2 Add a medicine
    1. Click "Add Medicine" on the Medicines page.
    2. Fill in the form in the popup. Fields with errors are shown in red.
    3. Click Save. The popup closes, a message appears at the bottom and the
       list refreshes.

4.3 Sell a medicine from the list
    1. Click "Sell" on a row. The button is disabled for expired or
       out-of-stock medicines.
    2. The popup shows the medicine, the stock left and the price.
    3. Enter the quantity. The total is shown next to it.
    4. Click "Save Sale". The stock in the grid is updated.

4.4 Sales page
     Shows all sales, newest first, with paging (10/50/100/200/500).
     "Add Sale" opens the same popup. Type at least 2 letters of the medicine
      name, pick it from the list, enter the quantity and click "Save Sale".

4.5 Messages you may see
    "Only X left in stock for ..."            -> lower the quantity
    "... is expired and cannot be sold."      -> the medicine is expired
    "... with this expiry date already exists" -> that batch is already added
    "Could not connect to the server ..."     -> the API is not running


5. API QUICK REFERENCE
----------------------
GET  /api/medicines?search=&filter=&sortBy=&sortDir=&page=&pageSize=
GET  /api/medicines/{id}
POST /api/medicines
     { "fullName": "Cetirizine Tablets", "brand": "Cipla",
       "expiryDate": "2027-06-30", "quantity": 50, "price": 24.50,
       "notes": "Store in a cool and dry place" }
GET  /api/sales?page=&pageSize=
POST /api/sales
     { "medicineId": 1, "quantity": 2 }

Sample requests are in backend/AbcPharmacy.Api/AbcPharmacy.Api.http.


