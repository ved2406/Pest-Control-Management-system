// --- global variables ---

const API = ''; // base url, empty = same server
let currentPage = 'dashboard';
// arrays to hold all the data we load from the api
let customers = [], pestTypes = [], bookings = [], technicians = [], treatments = [], reports = [];

// --- startup ---

document.addEventListener('DOMContentLoaded', () => {
    setupNavigation();
    setupModal();
    setupGlobalSearch();
    loadPage('dashboard'); // show dashboard first
});

// --- navigation (sidebar + hamburger menu) ---

function setupNavigation() {
    // make each nav link clickable
    document.querySelectorAll('.nav-item').forEach(item => {
        item.addEventListener('click', () => {
            const page = item.dataset.page;
            // remove active from all links then add it to the one clicked
            document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
            item.classList.add('active');
            loadPage(page);
            // close sidebar on mobile after clicking
            document.getElementById('sidebar').classList.remove('open');
        });
    });
    // hamburger button toggles sidebar
    document.getElementById('menuToggle').addEventListener('click', () => {
        document.getElementById('sidebar').classList.toggle('open');
    });
}

// --- global search bar ---

function setupGlobalSearch() {
    const input = document.getElementById('globalSearch');
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && input.value.trim()) {
            document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
            document.querySelector('[data-page="search"]').classList.add('active');
            loadSearchPage(input.value.trim());
        }
    });
}

// --- modal popup ---

function setupModal() {
    document.getElementById('modalClose').addEventListener('click', closeModal);
    // clicking the dark overlay also closes the modal
    document.getElementById('modalOverlay').addEventListener('click', (e) => {
        if (e.target === e.currentTarget) closeModal();
    });
}

function openModal(title, bodyHtml) {
    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalBody').innerHTML = bodyHtml;
    document.getElementById('modalOverlay').classList.add('show');
}

function closeModal() {
    document.getElementById('modalOverlay').classList.remove('show');
}

// --- fetch helpers (reusable api calls) ---

async function fetchJson(url) {
    const res = await fetch(API + url);
    if (!res.ok) return null;
    return res.json();
}

async function postJson(url, data) {
    const res = await fetch(API + url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
    return res;
}

async function putJson(url, data) {
    const res = await fetch(API + url, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
    return res;
}

async function deleteReq(url) {
    return fetch(API + url, { method: 'DELETE' });
}

// --- load all data from api ---

async function loadAllData() {
    // fetch everything at once with Promise.all
    const results = await Promise.all([
        fetchJson('/api/customers'),
        fetchJson('/api/pesttypes'),
        fetchJson('/api/bookings'),
        fetchJson('/api/technicians'),
        fetchJson('/api/treatments'),
        fetchJson('/api/inspectionreports')
    ]);
    customers = results[0] || [];
    pestTypes = results[1] || [];
    bookings = results[2] || [];
    technicians = results[3] || [];
    treatments = results[4] || [];
    reports = results[5] || [];
}

// --- page loader ---

async function loadPage(page) {
    currentPage = page;
    document.getElementById('pageTitle').textContent = getPageTitle(page);
    await loadAllData();

    const content = document.getElementById('content');
    switch (page) {
        case 'dashboard': content.innerHTML = renderDashboard(); break;
        case 'bookings': content.innerHTML = renderBookings(); setupBookingActions(); break;
        case 'customers': content.innerHTML = renderCustomers(); setupCustomerActions(); break;
        case 'technicians': content.innerHTML = renderTechnicians(); break;
        case 'pests': content.innerHTML = renderPests(); break;
        case 'treatments': content.innerHTML = renderTreatments(); break;
        case 'reports': content.innerHTML = renderReports(); break;
        case 'search': content.innerHTML = renderSearch(); setupSearchActions(); break;
    }
}

function loadSearchPage(query) {
    currentPage = 'search';
    document.getElementById('pageTitle').textContent = 'Search';
    const content = document.getElementById('content');
    content.innerHTML = renderSearch();
    // pre-fill the search box and run it
    const input = document.querySelector('.search-input-lg');
    if (input) {
        input.value = query;
        performSearch(query);
    }
    setupSearchActions();
}

// --- helpers ---

function getPageTitle(page) {
    const titles = {
        dashboard: 'Dashboard', bookings: 'Bookings', customers: 'Customers',
        technicians: 'Technicians', pests: 'Pest Types', treatments: 'Treatments',
        reports: 'Inspection Reports', search: 'Search'
    };
    return titles[page] || 'Dashboard';
}

// look up names by id
function getCustomerName(id) { const c = customers.find(x => x.id === id); return c ? c.name : 'Unknown'; }
function getPestName(id) { const p = pestTypes.find(x => x.id === id); return p ? p.name : 'Unknown'; }
function getTechName(id) { const t = technicians.find(x => x.id === id); return t ? t.name : 'Unknown'; }

// coloured badge for status/risk
function statusBadge(status) {
    const cls = status.replace(/\s/g, '').toLowerCase();
    return '<span class="badge badge-' + cls + '">' + status + '</span>';
}
function riskBadge(level) {
    return '<span class="badge badge-' + level.toLowerCase() + '">' + level + '</span>';
}

// --- dashboard ---

function renderDashboard() {
    // count bookings by status for the stat cards
    const active = bookings.filter(b => b.status === 'Confirmed' || b.status === 'In Progress').length;
    const completed = bookings.filter(b => b.status === 'Completed').length;
    const pending = bookings.filter(b => b.status === 'Pending').length;
    const availTechs = technicians.filter(t => t.available).length;
    const followUps = reports.filter(r => r.followUpNeeded).length;

    let html = '<div class="stats-grid">';
    html += statCard('Total Bookings', bookings.length, '', 'primary');
    html += statCard('Active', active, 'Confirmed & In Progress', 'info');
    html += statCard('Pending', pending, 'Awaiting confirmation', 'warning');
    html += statCard('Completed', completed, 'Successfully done', 'success');
    html += statCard('Customers', customers.length, 'Registered', 'primary');
    html += statCard('Technicians', availTechs + '/' + technicians.length, 'Available', 'success');
    html += statCard('Follow-ups', followUps, 'Reports needing action', 'danger');
    html += statCard('Treatments', treatments.length, 'Available products', 'info');
    html += '</div>';

    // upcoming bookings table (filter out completed, sort by date)
    html += '<div class="section-card"><div class="section-header"><h2>Upcoming Bookings</h2></div>';
    html += '<div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Date</th><th>Time</th><th>Customer</th><th>Pest</th><th>Technician</th><th>Status</th>';
    html += '</tr></thead><tbody>';
    const upcoming = bookings.filter(b => b.status !== 'Completed').sort((a, b) => a.date.localeCompare(b.date));
    upcoming.forEach(b => {
        html += '<tr><td>#' + b.id + '</td><td>' + b.date + '</td><td>' + b.time + '</td>';
        html += '<td>' + getCustomerName(b.customerId) + '</td><td>' + getPestName(b.pestTypeId) + '</td>';
        html += '<td>' + getTechName(b.technicianId) + '</td><td>' + statusBadge(b.status) + '</td></tr>';
    });
    if (upcoming.length === 0) {
        html += '<tr><td colspan="7" class="empty-state"><div class="empty-state-text">No upcoming bookings</div></td></tr>';
    }
    html += '</tbody></table></div></div>';

    // recent reports table
    html += '<div class="section-card"><div class="section-header"><h2>Recent Inspection Reports</h2></div>';
    html += '<div class="table-wrapper"><table><thead><tr>';
    html += '<th>Report</th><th>Date</th><th>Findings</th><th>Follow-up</th>';
    html += '</tr></thead><tbody>';
    reports.forEach(r => {
        html += '<tr><td>#' + r.id + '</td><td>' + r.reportDate + '</td>';
        html += '<td>' + (r.findings.length > 60 ? r.findings.substring(0, 60) + '...' : r.findings) + '</td>';
        html += '<td>' + (r.followUpNeeded ? '<span class="badge badge-high">Yes</span>' : '<span class="badge badge-low">No</span>') + '</td></tr>';
    });
    html += '</tbody></table></div></div>';

    return html;
}

function statCard(label, value, subtitle, color) {
    return '<div class="stat-card ' + color + '"><div class="stat-value">' + value + '</div><div class="stat-label">' + label + '</div></div>';
}

// --- bookings page ---

function renderBookings() {
    let html = '<div class="section-card"><div class="section-header"><h2>All Bookings</h2>';
    html += '<button class="btn btn-primary" onclick="openNewBookingModal()">+ New Booking</button>';
    html += '</div><div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Date</th><th>Time</th><th>Customer</th><th>Pest Type</th><th>Technician</th><th>Location</th><th>Status</th><th>Actions</th>';
    html += '</tr></thead><tbody>';
    bookings.forEach(b => {
        html += '<tr><td>#' + b.id + '</td><td>' + b.date + '</td><td>' + b.time + '</td>';
        html += '<td>' + getCustomerName(b.customerId) + '</td>';
        html += '<td>' + getPestName(b.pestTypeId) + '</td>';
        html += '<td>' + getTechName(b.technicianId) + '</td>';
        html += '<td>' + b.location + '</td>';
        html += '<td>' + statusBadge(b.status) + '</td>';
        html += '<td><div class="btn-group">';
        html += '<button class="btn btn-outline btn-sm" onclick="viewBooking(' + b.id + ')">View</button>';
        html += '<button class="btn btn-danger btn-sm" onclick="deleteBooking(' + b.id + ')">Delete</button>';
        html += '</div></td></tr>';
    });
    html += '</tbody></table></div></div>';
    return html;
}

function setupBookingActions() {}

function viewBooking(id) {
    const b = bookings.find(x => x.id === id);
    if (!b) return;
    let html = '<div class="detail-grid">';
    html += detailRow('Booking ID', '#' + b.id);
    html += detailRow('Date', b.date);
    html += detailRow('Time', b.time);
    html += detailRow('Customer', getCustomerName(b.customerId));
    html += detailRow('Pest Type', getPestName(b.pestTypeId));
    html += detailRow('Technician', getTechName(b.technicianId));
    html += detailRow('Location', b.location);
    html += detailRow('Status', statusBadge(b.status));
    html += detailRow('Notes', b.notes);
    html += '</div>';
    openModal('Booking #' + b.id, html);
}

function openNewBookingModal() {
    // build dropdown options from our data arrays
    let custOptions = customers.map(c => '<option value="' + c.id + '">' + c.name + '</option>').join('');
    let pestOptions = pestTypes.map(p => '<option value="' + p.id + '">' + p.name + '</option>').join('');
    let techOptions = technicians.map(t => '<option value="' + t.id + '">' + t.name + (t.available ? '' : ' (Unavailable)') + '</option>').join('');

    let html = '<form id="newBookingForm">';
    html += '<div class="form-row">';
    html += formSelect('Customer', 'bCustomerId', custOptions);
    html += formSelect('Pest Type', 'bPestTypeId', pestOptions);
    html += '</div>';
    html += '<div class="form-row">';
    html += formInput('Date', 'bDate', 'date');
    html += formInput('Time', 'bTime', 'time');
    html += '</div>';
    html += '<div class="form-row">';
    html += formSelect('Technician', 'bTechnicianId', techOptions);
    html += formSelect('Status', 'bStatus', '<option>Pending</option><option>Confirmed</option>');
    html += '</div>';
    html += formInput('Location', 'bLocation', 'text');
    html += formTextarea('Notes', 'bNotes');
    html += '<button type="submit" class="btn btn-primary" style="width:100%;margin-top:8px;">Create Booking</button>';
    html += '</form>';

    openModal('New Booking', html);
    document.getElementById('newBookingForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            id: 0,
            customerId: parseInt(document.getElementById('bCustomerId').value),
            pestTypeId: parseInt(document.getElementById('bPestTypeId').value),
            technicianId: parseInt(document.getElementById('bTechnicianId').value),
            date: document.getElementById('bDate').value,
            time: document.getElementById('bTime').value,
            status: document.getElementById('bStatus').value,
            location: document.getElementById('bLocation').value,
            notes: document.getElementById('bNotes').value
        };
        await postJson('/api/bookings', data);
        closeModal();
        loadPage('bookings');
    });
}

async function deleteBooking(id) {
    if (!confirm('Delete booking #' + id + '?')) return;
    await deleteReq('/api/bookings/' + id);
    loadPage('bookings');
}

// --- customers page ---

function renderCustomers() {
    let html = '<div class="section-card"><div class="section-header"><h2>All Customers</h2>';
    html += '<button class="btn btn-primary" onclick="openNewCustomerModal()">+ New Customer</button>';
    html += '</div><div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Name</th><th>Address</th><th>Phone</th><th>Email</th><th>Property</th><th>Actions</th>';
    html += '</tr></thead><tbody>';
    customers.forEach(c => {
        html += '<tr><td>#' + c.id + '</td><td>' + c.name + '</td><td>' + c.address + '</td>';
        html += '<td>' + c.phone + '</td><td>' + c.email + '</td><td>' + c.propertyType + '</td>';
        html += '<td><div class="btn-group">';
        html += '<button class="btn btn-outline btn-sm" onclick="viewCustomer(' + c.id + ')">View</button>';
        html += '<button class="btn btn-danger btn-sm" onclick="deleteCustomer(' + c.id + ')">Delete</button>';
        html += '</div></td></tr>';
    });
    html += '</tbody></table></div></div>';
    return html;
}

function setupCustomerActions() {}

function viewCustomer(id) {
    const c = customers.find(x => x.id === id);
    if (!c) return;
    const custBookings = bookings.filter(b => b.customerId === id);
    let html = '<div class="detail-grid">';
    html += detailRow('Name', c.name);
    html += detailRow('Address', c.address);
    html += detailRow('Phone', c.phone);
    html += detailRow('Email', c.email);
    html += detailRow('Property Type', c.propertyType);
    html += detailRow('Total Bookings', custBookings.length.toString());
    html += '</div>';
    if (custBookings.length > 0) {
        html += '<h3 style="margin-top:16px;margin-bottom:8px;font-size:0.95rem;">Booking History</h3>';
        html += '<table><thead><tr><th>Date</th><th>Pest</th><th>Status</th></tr></thead><tbody>';
        custBookings.forEach(b => {
            html += '<tr><td>' + b.date + '</td><td>' + getPestName(b.pestTypeId) + '</td><td>' + statusBadge(b.status) + '</td></tr>';
        });
        html += '</tbody></table>';
    }
    openModal(c.name, html);
}

function openNewCustomerModal() {
    let html = '<form id="newCustomerForm">';
    html += formInput('Name', 'cName', 'text');
    html += formInput('Address', 'cAddress', 'text');
    html += '<div class="form-row">';
    html += formInput('Phone', 'cPhone', 'tel');
    html += formInput('Email', 'cEmail', 'email');
    html += '</div>';
    html += formSelect('Property Type', 'cPropertyType', '<option>Residential</option><option>Commercial</option><option>Industrial</option>');
    html += '<button type="submit" class="btn btn-primary" style="width:100%;margin-top:8px;">Add Customer</button>';
    html += '</form>';

    openModal('New Customer', html);
    document.getElementById('newCustomerForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            id: 0,
            name: document.getElementById('cName').value,
            address: document.getElementById('cAddress').value,
            phone: document.getElementById('cPhone').value,
            email: document.getElementById('cEmail').value,
            propertyType: document.getElementById('cPropertyType').value
        };
        await postJson('/api/customers', data);
        closeModal();
        loadPage('customers');
    });
}

async function deleteCustomer(id) {
    if (!confirm('Delete customer #' + id + '?')) return;
    await deleteReq('/api/customers/' + id);
    loadPage('customers');
}

// --- technicians page ---

function renderTechnicians() {
    let html = '<div class="section-card"><div class="section-header"><h2>All Technicians</h2>';
    html += '<button class="btn btn-primary" onclick="openNewTechnicianModal()">+ New Technician</button>';
    html += '</div><div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Name</th><th>Specialisation</th><th>Phone</th><th>Email</th><th>Status</th><th>Actions</th>';
    html += '</tr></thead><tbody>';
    technicians.forEach(t => {
        html += '<tr><td>#' + t.id + '</td><td>' + t.name + '</td><td>' + t.specialisation + '</td>';
        html += '<td>' + t.phone + '</td><td>' + t.email + '</td>';
        html += '<td>' + (t.available ? '<span class="badge badge-available">Available</span>' : '<span class="badge badge-unavailable">Unavailable</span>') + '</td>';
        html += '<td><button class="btn btn-danger btn-sm" onclick="deleteTechnician(' + t.id + ')">Delete</button></td>';
        html += '</tr>';
    });
    html += '</tbody></table></div></div>';
    return html;
}

function openNewTechnicianModal() {
    let html = '<form id="newTechForm">';
    html += formInput('Name', 'tName', 'text');
    html += formInput('Specialisation', 'tSpec', 'text');
    html += '<div class="form-row">';
    html += formInput('Phone', 'tPhone', 'tel');
    html += formInput('Email', 'tEmail', 'email');
    html += '</div>';
    html += formSelect('Available', 'tAvailable', '<option value="true">Available</option><option value="false">Unavailable</option>');
    html += '<button type="submit" class="btn btn-primary" style="width:100%;margin-top:8px;">Add Technician</button>';
    html += '</form>';

    openModal('New Technician', html);
    document.getElementById('newTechForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            id: 0,
            name: document.getElementById('tName').value,
            specialisation: document.getElementById('tSpec').value,
            phone: document.getElementById('tPhone').value,
            email: document.getElementById('tEmail').value,
            available: document.getElementById('tAvailable').value === 'true'
        };
        await postJson('/api/technicians', data);
        closeModal();
        loadPage('technicians');
    });
}

async function deleteTechnician(id) {
    if (!confirm('Delete technician #' + id + '?')) return;
    await deleteReq('/api/technicians/' + id);
    loadPage('technicians');
}

// --- pest types page ---

function renderPests() {
    let html = '<div class="section-card"><div class="section-header"><h2>All Pest Types</h2>';
    html += '<button class="btn btn-primary" onclick="openNewPestModal()">+ New Pest Type</button>';
    html += '</div><div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Name</th><th>Category</th><th>Description</th><th>Risk</th><th>Actions</th>';
    html += '</tr></thead><tbody>';
    pestTypes.forEach(p => {
        html += '<tr><td>#' + p.id + '</td><td>' + p.name + '</td><td>' + p.category + '</td>';
        html += '<td>' + p.description + '</td>';
        html += '<td>' + riskBadge(p.riskLevel) + '</td>';
        html += '<td><button class="btn btn-danger btn-sm" onclick="deletePest(' + p.id + ')">Delete</button></td></tr>';
    });
    html += '</tbody></table></div></div>';
    return html;
}

function openNewPestModal() {
    let html = '<form id="newPestForm">';
    html += formInput('Name', 'pName', 'text');
    html += '<div class="form-row">';
    html += formSelect('Category', 'pCategory', '<option>Insects</option><option>Rodents</option><option>Birds</option><option>Wildlife</option><option>Other</option>');
    html += formSelect('Risk Level', 'pRisk', '<option>Low</option><option>Medium</option><option>High</option>');
    html += '</div>';
    html += formTextarea('Description', 'pDesc');
    html += '<button type="submit" class="btn btn-primary" style="width:100%;margin-top:8px;">Add Pest Type</button>';
    html += '</form>';

    openModal('New Pest Type', html);
    document.getElementById('newPestForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            id: 0,
            name: document.getElementById('pName').value,
            category: document.getElementById('pCategory').value,
            description: document.getElementById('pDesc').value,
            riskLevel: document.getElementById('pRisk').value
        };
        await postJson('/api/pesttypes', data);
        closeModal();
        loadPage('pests');
    });
}

async function deletePest(id) {
    if (!confirm('Delete pest type #' + id + '?')) return;
    await deleteReq('/api/pesttypes/' + id);
    loadPage('pests');
}

// --- treatments page ---

function renderTreatments() {
    let html = '<div class="section-card"><div class="section-header"><h2>All Treatments</h2></div>';
    html += '<div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Product</th><th>Method</th><th>Target Pest</th><th>Safety Info</th>';
    html += '</tr></thead><tbody>';
    treatments.forEach(t => {
        html += '<tr><td>#' + t.id + '</td><td>' + t.productName + '</td><td>' + t.method + '</td>';
        html += '<td>' + getPestName(t.targetPestTypeId) + '</td>';
        html += '<td>' + (t.safetyInfo.length > 60 ? t.safetyInfo.substring(0, 60) + '...' : t.safetyInfo) + '</td></tr>';
    });
    html += '</tbody></table></div></div>';
    return html;
}

// --- inspection reports page ---

function renderReports() {
    let html = '<div class="section-card"><div class="section-header"><h2>All Inspection Reports</h2></div>';
    html += '<div class="table-wrapper"><table><thead><tr>';
    html += '<th>ID</th><th>Booking</th><th>Date</th><th>Findings</th><th>Follow-up</th><th>Actions</th>';
    html += '</tr></thead><tbody>';
    reports.forEach(r => {
        html += '<tr><td>#' + r.id + '</td><td>#' + r.bookingId + '</td><td>' + r.reportDate + '</td>';
        html += '<td>' + (r.findings.length > 50 ? r.findings.substring(0, 50) + '...' : r.findings) + '</td>';
        html += '<td>' + (r.followUpNeeded ? '<span class="badge badge-high">Yes</span>' : '<span class="badge badge-low">No</span>') + '</td>';
        html += '<td><button class="btn btn-outline btn-sm" onclick="viewReport(' + r.id + ')">View</button></td></tr>';
    });
    html += '</tbody></table></div></div>';
    return html;
}

function viewReport(id) {
    const r = reports.find(x => x.id === id);
    if (!r) return;
    let html = '<div class="detail-grid">';
    html += detailRow('Report ID', '#' + r.id);
    html += detailRow('Booking', '#' + r.bookingId);
    html += detailRow('Date', r.reportDate);
    html += detailRow('Follow-up Needed', r.followUpNeeded ? 'Yes' : 'No');
    html += '</div>';
    html += '<h3 style="margin-top:16px;margin-bottom:6px;font-size:0.95rem;">Findings</h3>';
    html += '<p style="font-size:0.9rem;color:var(--gray-700);line-height:1.6;">' + r.findings + '</p>';
    html += '<h3 style="margin-top:16px;margin-bottom:6px;font-size:0.95rem;">Recommendations</h3>';
    html += '<p style="font-size:0.9rem;color:var(--gray-700);line-height:1.6;">' + r.recommendations + '</p>';
    openModal('Inspection Report #' + r.id, html);
}

// --- search page ---

function renderSearch() {
    let html = '<input type="text" class="search-input-lg" id="searchInput" placeholder="Search customers, bookings, pests, technicians, treatments, reports..." />';
    html += '<div class="section-card" id="searchResults"><div class="section-body"><div class="empty-state">';
    html += '<div class="empty-state-icon">&#128269;</div><div class="empty-state-text">Type to search across all data</div></div></div></div>';
    return html;
}

function setupSearchActions() {
    const input = document.getElementById('searchInput');
    if (!input) return;
    let timer;
    // debounce - wait 300ms after user stops typing before searching
    input.addEventListener('input', () => {
        clearTimeout(timer);
        timer = setTimeout(() => {
            if (input.value.trim().length > 0) {
                performSearch(input.value.trim());
            }
        }, 300);
    });
    input.focus();
}

async function performSearch(query) {
    const data = await fetchJson('/api/search?q=' + encodeURIComponent(query));
    const container = document.getElementById('searchResults');
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="section-body"><div class="empty-state"><div class="empty-state-icon">&#128533;</div><div class="empty-state-text">No results found for "' + query + '"</div></div></div>';
        return;
    }
    let html = '';
    data.forEach(r => {
        html += '<div class="search-result-item" style="cursor:pointer;" onclick="openSearchResult(\'' + r.category + '\',' + r.id + ')">';
        html += '<span class="search-result-category">' + r.category + '</span>';
        html += '<div class="search-result-info">';
        html += '<div class="search-result-title">' + r.title + '</div>';
        html += '<div class="search-result-desc">' + r.description + '</div>';
        html += '</div></div>';
    });
    container.innerHTML = html;
}

// --- form builder helpers ---

function formInput(label, id, type) {
    return '<div class="form-group"><label for="' + id + '">' + label + '</label><input type="' + type + '" id="' + id + '" required /></div>';
}

function formSelect(label, id, options) {
    return '<div class="form-group"><label for="' + id + '">' + label + '</label><select id="' + id + '">' + options + '</select></div>';
}

function formTextarea(label, id) {
    return '<div class="form-group"><label for="' + id + '">' + label + '</label><textarea id="' + id + '"></textarea></div>';
}

function detailRow(label, value) {
    return '<div class="detail-label">' + label + '</div><div class="detail-value">' + value + '</div>';
}

// --- search result click handler ---

function openSearchResult(category, id) {
    switch (category) {
        case 'Customer':
            loadAllData().then(() => viewCustomer(id));
            break;
        case 'Booking':
            loadAllData().then(() => viewBooking(id));
            break;
        case 'Technician':
            loadAllData().then(() => viewTechnician(id));
            break;
        case 'Pest Type':
            loadAllData().then(() => viewPest(id));
            break;
        case 'Treatment':
            loadAllData().then(() => viewTreatment(id));
            break;
        case 'Inspection Report':
            loadAllData().then(() => viewReport(id));
            break;
    }
}

// --- detail views (opened from search results or tables) ---

function viewTechnician(id) {
    const t = technicians.find(x => x.id === id);
    if (!t) return;
    const techBookings = bookings.filter(b => b.technicianId === id);
    let html = '<div class="detail-grid">';
    html += detailRow('Name', t.name);
    html += detailRow('Specialisation', t.specialisation);
    html += detailRow('Phone', t.phone);
    html += detailRow('Email', t.email);
    html += detailRow('Status', t.available ? '<span class="badge badge-available">Available</span>' : '<span class="badge badge-unavailable">Unavailable</span>');
    html += detailRow('Assigned Bookings', techBookings.length.toString());
    html += '</div>';
    if (techBookings.length > 0) {
        html += '<h3 style="margin-top:16px;margin-bottom:8px;font-size:0.95rem;">Assigned Bookings</h3>';
        html += '<table><thead><tr><th>Date</th><th>Customer</th><th>Pest</th><th>Status</th></tr></thead><tbody>';
        techBookings.forEach(b => {
            html += '<tr><td>' + b.date + '</td><td>' + getCustomerName(b.customerId) + '</td><td>' + getPestName(b.pestTypeId) + '</td><td>' + statusBadge(b.status) + '</td></tr>';
        });
        html += '</tbody></table>';
    }
    openModal(t.name, html);
}

function viewPest(id) {
    const p = pestTypes.find(x => x.id === id);
    if (!p) return;
    const pestTreatments = treatments.filter(t => t.targetPestTypeId === id);
    let html = '<div class="detail-grid">';
    html += detailRow('Name', p.name);
    html += detailRow('Category', p.category);
    html += detailRow('Risk Level', riskBadge(p.riskLevel));
    html += detailRow('Description', p.description);
    html += '</div>';
    if (pestTreatments.length > 0) {
        html += '<h3 style="margin-top:16px;margin-bottom:8px;font-size:0.95rem;">Available Treatments</h3>';
        html += '<table><thead><tr><th>Product</th><th>Method</th><th>Safety</th></tr></thead><tbody>';
        pestTreatments.forEach(t => {
            html += '<tr><td>' + t.productName + '</td><td>' + t.method + '</td><td>' + (t.safetyInfo.length > 40 ? t.safetyInfo.substring(0, 40) + '...' : t.safetyInfo) + '</td></tr>';
        });
        html += '</tbody></table>';
    }
    openModal(p.name, html);
}

function viewTreatment(id) {
    const t = treatments.find(x => x.id === id);
    if (!t) return;
    let html = '<div class="detail-grid">';
    html += detailRow('Product', t.productName);
    html += detailRow('Method', t.method);
    html += detailRow('Target Pest', getPestName(t.targetPestTypeId));
    html += detailRow('Safety Info', t.safetyInfo);
    html += '</div>';
    openModal(t.productName, html);
}

// --- ai chat widget ---

(function() {
    const fab = document.getElementById('chatFab');
    const widget = document.getElementById('chatWidget');
    const closeBtn = document.getElementById('chatClose');
    const input = document.getElementById('chatInput');
    const sendBtn = document.getElementById('chatSend');
    const messages = document.getElementById('chatMessages');

    if (!fab || !widget) return;

    // open chat when clicking the floating button
    fab.addEventListener('click', () => {
        fab.classList.add('hidden');
        widget.classList.add('open');
        input.focus();
    });

    // close chat
    closeBtn.addEventListener('click', () => {
        widget.classList.remove('open');
        fab.classList.remove('hidden');
    });

    function addMessage(text, sender, armName) {
        const div = document.createElement('div');
        div.className = 'chat-msg ' + sender;
        let html = '<div class="chat-msg-bubble">' + escapeHtml(text) + '</div>';
        if (armName && armName !== 'general') {
            html += '<div class="chat-msg-arm">' + armName + '</div>';
        }
        div.innerHTML = html;
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
    }

    function addTypingIndicator() {
        const div = document.createElement('div');
        div.className = 'chat-typing';
        div.id = 'chatTyping';
        div.innerHTML = '<span></span><span></span><span></span>';
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
    }

    function removeTypingIndicator() {
        const el = document.getElementById('chatTyping');
        if (el) el.remove();
    }

    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        addMessage(text, 'user');
        input.value = '';
        addTypingIndicator();

        try {
            const res = await fetch('/api/agent', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: text })
            });
            removeTypingIndicator();

            if (res.ok) {
                const data = await res.json();
                addMessage(data.message, 'bot', data.arm);
            } else {
                addMessage('Sorry, something went wrong. Please try again.', 'bot');
            }
        } catch (err) {
            removeTypingIndicator();
            addMessage('Unable to reach the server. Is the API running?', 'bot');
        }
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') sendMessage();
    });

    function escapeHtml(text) {
        // prevent html injection by using textContent
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
})();
