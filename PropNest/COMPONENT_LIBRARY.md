# PropNest UI Component Library

## Quick Reference Guide for Developers

### Navigation & Layout

#### Page Header
```html
<div class="page-header">
	<h1>Page Title</h1>
	<p class="text-muted">Subtitle or description</p>
</div>
```

#### Search & Filter Bar
```html
<div class="search-filter-bar">
	<div class="row g-3 align-items-end">
		<div class="col-md-8">
			<form method="get">
				<div class="input-group">
					<span class="input-group-text bg-white border-end-0">🔍</span>
					<input type="text" class="form-control border-start-0" placeholder="Search..." />
				</div>
			</form>
		</div>
		<div class="col-md-4 d-flex gap-2">
			<button class="btn btn-outline-secondary flex-grow-1">Clear</button>
			<button class="btn btn-apple flex-grow-1">Add New</button>
		</div>
	</div>
</div>
```

---

### Cards & Containers

#### Apple Card
```html
<div class="apple-card p-4">
	<h5 class="card-title">Card Title</h5>
	<p class="text-muted">Card content goes here...</p>
</div>
```

#### Metric Card (Dashboard)
```html
<div class="metric-card apple-card">
	<div class="text-center">
		<div class="metric-icon">💰</div>
		<h6 class="metric-label">Total Revenue</h6>
		<h3 class="metric-value" style="color: #34C759;">$45,230</h3>
		<p class="metric-subtitle">This month</p>
	</div>
</div>
```

#### Card Section
```html
<div class="card-section">
	<h5>Section Title</h5>
	<p>Section content...</p>
</div>
```

---

### Buttons

#### Primary Button
```html
<button class="btn btn-apple">
	Action Button
</button>
```

#### Secondary Button
```html
<button class="btn btn-outline-secondary">
	Secondary Action
</button>
```

#### Danger Button
```html
<button class="btn btn-danger">
	Delete
</button>
```

#### Button Group
```html
<div class="btn-group-custom">
	<a class="btn btn-sm">View</a>
	<a class="btn btn-sm">Edit</a>
	<a class="btn btn-sm btn-danger">Delete</a>
</div>
```

---

### Forms

#### Text Input
```html
<div class="form-group">
	<label for="name" class="form-label">Full Name</label>
	<input type="text" class="form-control" id="name" placeholder="Enter your name" />
</div>
```

#### Select Input
```html
<div class="form-group">
	<label for="status" class="form-label">Status</label>
	<select class="form-control" id="status">
		<option>Select status...</option>
		<option>Active</option>
		<option>Inactive</option>
	</select>
</div>
```

#### Form Group with Validation
```html
<div class="form-group">
	<label for="email" class="form-label">Email Address</label>
	<input type="email" class="form-control is-invalid" id="email" />
	<div class="invalid-feedback">
		Please provide a valid email address.
	</div>
</div>
```

---

### Tables

#### Basic Table
```html
<div class="apple-card overflow-auto">
	<table class="table table-hover mb-0">
		<thead>
			<tr>
				<th>Column 1</th>
				<th>Column 2</th>
				<th>Actions</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>Data 1</td>
				<td>Data 2</td>
				<td>
					<div class="btn-group-custom">
						<a class="btn btn-sm">Edit</a>
						<a class="btn btn-sm btn-danger">Delete</a>
					</div>
				</td>
			</tr>
		</tbody>
	</table>
</div>
```

---

### Status & Badges

#### Status Badge - Active
```html
<span class="status-badge status-active">● Active</span>
```

#### Status Badge - Inactive
```html
<span class="status-badge status-inactive">● Inactive</span>
```

#### Status Badge - Pending
```html
<span class="status-badge status-pending">● Pending</span>
```

#### Status Badge - Completed
```html
<span class="status-badge status-completed">✓ Completed</span>
```

#### Status Badge - Error
```html
<span class="status-badge status-error">✕ Error</span>
```

#### Info Badge
```html
<span class="badge bg-info">New</span>
```

---

### Alerts & Messages

#### Success Alert
```html
<div class="alert alert-success">
	✓ Operation completed successfully!
</div>
```

#### Error Alert
```html
<div class="alert alert-danger">
	✕ An error occurred. Please try again.
</div>
```

#### Warning Alert
```html
<div class="alert alert-warning">
	⚠️ Please review the following items.
</div>
```

#### Info Alert
```html
<div class="alert alert-info">
	ℹ️ Here's some helpful information.
</div>
```

---

### Empty States

#### Empty List
```html
<div class="empty-state">
	<div class="empty-state-icon">📋</div>
	<h4>No Results Found</h4>
	<p>There are currently no items. Create your first item to get started.</p>
	<a href="#" class="btn btn-apple">+ Create New</a>
</div>
```

---

### Navigation

#### Breadcrumb
```html
<nav aria-label="breadcrumb">
	<ol class="breadcrumb">
		<li class="breadcrumb-item"><a href="#">Home</a></li>
		<li class="breadcrumb-item"><a href="#">Tenants</a></li>
		<li class="breadcrumb-item active">John Doe</li>
	</ol>
</nav>
```

#### Pagination
```html
<nav aria-label="Page navigation">
	<ul class="pagination justify-content-center">
		<li class="page-item"><a class="page-link" href="#">Previous</a></li>
		<li class="page-item"><a class="page-link" href="#">1</a></li>
		<li class="page-item active"><a class="page-link" href="#">2</a></li>
		<li class="page-item"><a class="page-link" href="#">3</a></li>
		<li class="page-item"><a class="page-link" href="#">Next</a></li>
	</ul>
</nav>
```

---

### Modals

#### Modal Dialog
```html
<div class="modal fade" id="exampleModal" tabindex="-1">
	<div class="modal-dialog">
		<div class="modal-content">
			<div class="modal-header">
				<h5 class="modal-title">Modal Title</h5>
				<button type="button" class="btn-close" data-bs-dismiss="modal"></button>
			</div>
			<div class="modal-body">
				Modal content goes here...
			</div>
			<div class="modal-footer">
				<button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Close</button>
				<button type="button" class="btn btn-apple">Save Changes</button>
			</div>
		</div>
	</div>
</div>
```

---

### Dropdowns

#### Dropdown Menu
```html
<div class="dropdown">
	<button class="btn btn-apple dropdown-toggle" type="button" data-bs-toggle="dropdown">
		Menu
	</button>
	<ul class="dropdown-menu">
		<li><a class="dropdown-item" href="#">Action</a></li>
		<li><a class="dropdown-item" href="#">Another action</a></li>
		<li><hr class="dropdown-divider"></li>
		<li><a class="dropdown-item" href="#">Delete</a></li>
	</ul>
</div>
```

---

### Lists with Icons

#### Contact Information
```html
<div class="card-section">
	<h5>Contact Information</h5>
	<div class="mb-3">
		<span class="me-2">📧</span>
		<a href="mailto:john@example.com">john@example.com</a>
	</div>
	<div class="mb-3">
		<span class="me-2">📱</span>
		<a href="tel:+1234567890">+1 (234) 567-890</a>
	</div>
	<div class="mb-0">
		<span class="me-2">📍</span>
		<span>123 Main Street, City, State</span>
	</div>
</div>
```

---

### Grid Layouts

#### 2-Column Layout
```html
<div class="row g-4">
	<div class="col-md-6">
		<div class="apple-card p-4">Content 1</div>
	</div>
	<div class="col-md-6">
		<div class="apple-card p-4">Content 2</div>
	</div>
</div>
```

#### 3-Column Layout (Dashboard)
```html
<div class="row g-4">
	<div class="col-md-6 col-lg-3">
		<div class="metric-card apple-card">Metric 1</div>
	</div>
	<div class="col-md-6 col-lg-3">
		<div class="metric-card apple-card">Metric 2</div>
	</div>
	<div class="col-md-6 col-lg-3">
		<div class="metric-card apple-card">Metric 3</div>
	</div>
	<div class="col-md-6 col-lg-3">
		<div class="metric-card apple-card">Metric 4</div>
	</div>
</div>
```

---

### Text Utilities

#### Muted Text
```html
<p class="text-muted">This is secondary text</p>
```

#### Uppercase Label
```html
<label class="text-uppercase fw-bold" style="letter-spacing: 0.05em;">
	Label
</label>
```

#### Text with Code
```html
<code class="bg-light px-2 py-1 rounded">CODE_SAMPLE</code>
```

---

### Spacing & Layout

#### Container with Padding
```html
<div style="padding: 2rem;">
	Content with spacing
</div>
```

#### Margin Bottom (4 levels)
```html
<div class="mb-3">Small spacing</div>
<div class="mb-4">Medium spacing</div>
<div class="mb-5">Large spacing</div>
```

#### Gap Between Items
```html
<div class="d-flex gap-2">
	<button>Button 1</button>
	<button>Button 2</button>
</div>
```

---

### Responsive Classes

#### Hide/Show Based on Screen Size
```html
<!-- Hide on small screens, show on medium+ -->
<div class="d-none d-md-block">Desktop only</div>

<!-- Show on small screens, hide on medium+ -->
<div class="d-md-none">Mobile only</div>
```

#### Flex Layout
```html
<div class="d-flex justify-content-between align-items-center">
	<div>Left</div>
	<div>Right</div>
</div>
```

#### Grid with Responsive Gap
```html
<div class="row g-3 g-md-4">
	<div class="col-12 col-md-6">Item 1</div>
	<div class="col-12 col-md-6">Item 2</div>
</div>
```

---

## Color Usage Guidelines

### Use Primary Blue (#0071e3) for:
- Primary action buttons
- Links and navigation
- Focused states
- Important highlights

### Use Green (#34C759) for:
- Success messages
- Active status indicators
- Positive actions

### Use Orange (#FF9500) for:
- Warnings
- Pending items
- Attention-needed states

### Use Red (#FF3B30) for:
- Errors
- Dangerous actions (delete)
- Failed operations

### Use Cyan (#5AC8FA) for:
- Information
- Neutral alerts
- Secondary highlights

---

## Animation Examples

### Fade In on Load
```css
animation: fadeInUp 0.6s cubic-bezier(0.25, 0.8, 0.25, 1) forwards;
```

### Hover Lift Effect
```css
transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);

&:hover {
	transform: translateY(-8px) scale(1.02);
	box-shadow: 0 20px 40px rgba(0, 0, 0, 0.12);
}
```

### Smooth Color Transition
```css
transition: background-color 0.2s ease, color 0.2s ease;
```

---

## Accessibility Tips

1. Always maintain proper color contrast ratios
2. Use meaningful alt text for emoji icons
3. Ensure focusable elements are keyboard accessible
4. Use semantic HTML (buttons, links, headings)
5. Test with screen readers
6. Provide clear focus indicators

---

## Performance Notes

- All animations use CSS transforms for GPU acceleration
- Debounce rapid interactions
- Use lazy loading for images
- Minimize CSS specificity
- Use CSS variables for theming

---

**Last Updated**: 2024
**Component Library Version**: 1.0
