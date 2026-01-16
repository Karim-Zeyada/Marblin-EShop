Software Requirements Specification (SRS)
Project Name: MARBLIN – Marble & Stone Luxury E‑Shop
Version: 1.2
Date: December 3, 2025
 
1. Introduction
1.1 Purpose
This document defines the functional and non‑functional requirements for the “MARBLIN – Marble & Stone Luxury E‑Shop” web application. The system enables a marble and stone startup to sell high‑end handcrafted stone artifacts using a configurable deposit‑based payment model with guest‑only checkout and a single admin back office. It is the main reference for design, implementation, and testing.
1.2 Scope
The software is a web‑based e‑commerce platform.
•	Front‑Office (Store):
Product catalog browsing, variant selection, cart, guest checkout with configurable deposit, manual transfer payment proof, custom product request forms, and public order tracking.
•	Back‑Office (Admin Panel):
Admin dashboard for inventory management, order workflow, deposit configuration, viewing custom requests, basic analytics, and limited content management for the home page.
The initial release targets a Minimum Viable Product (MVP) focusing on guest buyers and a single business owner/admin.
1.3 Definitions and Acronyms
•	MVP: Minimum Viable Product.
•	CMS: Content Management System (limited scope in this project).
•	Guest Checkout: Purchasing without creating a permanent user account.
•	SKU: Stock Keeping Unit (product identifier).
•	Deposit: A configurable percentage of the order total paid upfront to confirm an order.
•	Manual Transfer: Off‑platform payment via Instapay or Vodafone Cash, confirmed by user‑submitted Transaction ID or receipt image.
1.4 Overview
Section 2 describes the overall system context and users. Section 3 lists functional requirements grouped by modules. Section 4 provides a high‑level use case view. Section 5 covers non‑functional requirements.
 
2. Overall Description
2.1 User Classes and Characteristics
•	Guest Customer
o	Luxury‑oriented buyer who values privacy, simplicity, and aesthetics.
o	Interacts via mobile and desktop browsers.
o	Does not create an account; uses guest checkout and order tracking with Order ID + Email.
•	Administrator (Owner)
o	Single non‑technical business owner.
o	Needs an intuitive, web‑based admin panel to:
	Manage products, variants, and categories.
	Configure deposit percentage.
	Verify manual payments and update order statuses.
	View custom product requests.
	Edit limited home page content.
2.2 Product Perspective
The system is a standalone web application. It:
•	Uses a database for products, orders, payments, and custom requests.
•	Integrates with external email and SMS services for notifications.
•	Uses semi‑automated payment verification: customers pay via manual transfer and submit proof; admin verifies and updates status.
2.3 Product Functions (High Level)
•	Display a visually rich catalog of stone products with variants.
•	Allow users to add items to a cart and complete a guest checkout.
•	Automatically calculate the deposit based on an admin‑configured percentage and clearly display deposit and remaining balance.
•	Accept manual transfer payment proofs (Transaction ID or receipt image).
•	Provide a dedicated Custom Product Request page with a structured form.
•	Provide an order tracking portal with a 4‑step status timeline.
•	Provide an admin panel for product management, order workflow, deposit configuration, and basic analytics.
2.4 Constraints
•	Web‑based, responsive (mobile‑first) UI.
•	Single admin role; no multi‑role/permissions management required in MVP.
•	No customer registration, login, or user dashboard in MVP.
•	Payments are manual transfers; no real‑time payment gateway integration in MVP.
 
3. System Features (Functional Requirements)
3.1 Module 1: Catalog & Discovery
FR‑01 View Products
The system shall allow guest users to view products in a catalog with high‑resolution image galleries and key details (name, price, material, size).
FR‑01.1 Signature Pieces (Home Page)
The Home Page shall feature a “Signature Pieces” section showing a curated grid of high‑value products with links to product detail pages.
FR‑02 Filter & Sort
The system shall allow users to filter products by:
•	Category (e.g., Tables, Countertops, Decorative, etc.)
•	Availability (e.g., In Stock, Made to Order)
The system shall support sorting (e.g., by price, newest).
FR‑03 Variant Selection
On product detail pages, users shall be able to select product variants (e.g., Material, Size). The displayed price shall update dynamically based on the selected variant.
 
3.2 Module 2: Cart, Checkout & Manual Payment
FR‑05 Add to Cart
Users shall be able to add one or more products (with selected variants) to a session‑based shopping cart without creating an account.
FR‑06 Configurable Deposit Calculation
•	The system shall calculate a Deposit Amount as a percentage of the cart total.
•	The deposit percentage shall be configurable by the Administrator in the admin panel (default 10%).
•	The checkout UI shall clearly display:
o	“Deposit: X%”
o	“Amount due now: [Deposit Amount]”
o	“Remaining balance: [Total – Deposit Amount]”.
FR‑07 Guest Checkout Form
The checkout form shall collect:
•	Full Name
•	Phone Number
•	Email Address
•	Shipping Address (address line, city, region, country, postal code)
No account creation or login shall be required.
FR‑08 Manual Payment Proof Submission
During checkout, the system shall:
•	Display clear instructions for paying via Instapay/Vodafone Cash (including target account details).
•	Require the user to either:
o	Enter a Transaction ID, or
o	Upload a payment receipt image.
This proof shall be stored with the order record for later admin verification.
 
3.3 Module 3: Custom Product Requests
FR‑09 Custom Product Request Page
The system shall provide a dedicated “Custom Product Request” page accessible from the main navigation and/or home page.
FR‑09.1 Custom Request Form Fields
The form shall allow the user to submit:
•	Full Name
•	Phone Number
•	Email Address
•	Product Category (e.g., Table, Countertop, Furniture, Other)
•	Desired Dimensions (free text)
•	Preferred Material (e.g., Black Marble, White Marble, Travertine, Not Sure)
•	Budget Range (optional)
•	Project Timeline (optional)
•	Additional Notes / Description (required)
•	Optionally upload one or more inspiration images
FR‑09.2 Custom Request Storage
Submitted custom requests shall be stored in the database as “Quote / Custom Request” records, accessible in the admin panel.
 
3.4 Module 4: Cart Page Recommendations
FR‑04 Cart Recommendations
On the Cart page only, the system shall display a “You Might Also Like” section with recommended products based on the category of items currently in the cart.
 
3.5 Module 5: Order Tracking
FR‑10 Public Tracking Portal
The system shall provide a public “Track Order” page.
FR‑11 Secure Access to Order Details
To view order details, the user must enter:
•	Order ID, and
•	Email Address used at checkout.
Only matching combinations shall return order information.
FR‑12 Status Timeline and Summary
The tracking page shall display:
•	Order summary: products, variants, quantities, total price.
•	Financial summary: deposit amount, remaining balance, and whether deposit has been verified.
•	A visual 4‑step status timeline:
a.	Pending Deposit
b.	In Production
c.	Awaiting Balance
d.	Shipped
Where available, each step shall show the date/time it was reached.
 
3.6 Module 6: Admin Management
FR‑13 Admin Dashboard Overview
After secure login, the Administrator shall see a dashboard with:
•	Today’s and recent sales (total order value and deposit collected).
•	Count of active orders by status (Pending Deposit, In Production, Awaiting Balance, Shipped).
•	A simple revenue chart over a selected period.
FR‑14 Inventory Management (CRUD)
The Administrator shall be able to:
•	Create, read, update, and delete products.
•	Manage product variants (e.g., material/size combinations and prices).
•	Manage categories.
FR‑14.1 Signature Pieces Flag
The Administrator shall be able to flag/unflag products as “Signature Pieces” so they appear in the home page Signature section.
FR‑15 Limited CMS (Home Content)
The Administrator shall be able to edit the following content via the admin panel:
•	Home page hero text (headline, subheadline).
•	Home page value statements/short descriptions.
•	Home page process/steps text.
Other site content (materials descriptions, FAQs, etc.) is static in the MVP.
FR‑16 Order Workflow Management
The Administrator shall be able to:
•	View a list of all orders with key information (customer, total, deposit, current status, payment proof).
•	Open an individual order to view:
o	Customer details.
o	Ordered items and variants.
o	Deposit calculation.
o	Payment proof (Transaction ID and/or receipt image).
o	Current status and history.
•	Update the order status through the allowed steps:
o	Pending Deposit → In Production → Awaiting Balance → Shipped.
•	Mark deposit as verified after checking the proof.
FR‑16.1 Custom Request Management
The Administrator shall be able to view a list of all custom product requests with filters and open each request to see details and attached inspiration images.
FR‑17 Basic Analytics
The system shall provide simple analytics views, including:
•	Top‑selling products (by units sold).
•	Top categories by revenue.
•	Total deposits collected over a selected time period.
FR‑18 Deposit Configuration
The Administrator shall be able to configure the global deposit percentage used for calculation (e.g., 10%, 20%, 50%). Changes shall apply to new orders only.
 
3.7 Module 7: Notifications
FR‑19 SMS Confirmation
On successful checkout (order created), the system shall send an SMS to the customer’s phone number containing at minimum:
•	A thank‑you message.
•	The Order ID.
•	A link to the Track Order page (if feasible).
FR‑20 Email Updates
The system shall send transactional emails to the customer:
•	On order creation (order summary and deposit information).
•	When the order status changes (e.g., “In Production”, “Awaiting Balance”, “Shipped”).
 
4. Use Case Overview (Informal)
•	Use Case UC1: Browse & Filter Products
•	UC2: Select Variants and View Details
•	UC3: Add to Cart and View Cart (with recommendations)
•	UC4: Checkout as Guest, submit manual transfer proof
•	UC5: Submit Custom Product Request
•	UC6: Track Order Status
•	UC7: Admin Login & View Dashboard
•	UC8: Manage Inventory & Signature Pieces
•	UC9: Configure Deposit Percentage
•	UC10: Verify Deposits & Update Order Status
•	UC11: View Analytics
•	UC12: Edit Home Page Content
 
5. Non‑Functional Requirements
5.1 Performance
•	NFR‑01: The home page shall load in under 2 seconds on a standard 4G mobile connection under typical load.
•	NFR‑02: Product and gallery images shall be optimized (e.g., WebP/responsive images) to maintain high visual quality without significantly degrading load times.
5.2 Security
•	NFR‑03: The admin panel shall be protected by secure authentication using session‑based login.
•	NFR‑04: All user inputs (checkout, custom request, tracking form) shall be validated and sanitized to mitigate SQL injection and XSS.
•	NFR‑05: Order details in the tracking portal shall not be guessable via URL; successful access requires a correct combination of Order ID and Email.
5.3 Usability
•	NFR‑06: The UI shall follow a mobile‑first responsive design, working well on smartphones, tablets, and desktops.
•	NFR‑07: Visual design shall follow a “Minimalist Luxury” aesthetic, with a clean black/white/gray palette, generous whitespace, and high‑quality imagery consistent with the MARBLIN brand.
5.4 Reliability & Data Integrity
•	NFR‑08: Inventory updates shall be atomic to prevent overselling items, particularly for low‑stock items (e.g., stock = 1).
•	NFR‑09: The system shall handle transient failures of SMS/Email providers gracefully (e.g., logging failed sends and allowing retry).
 
