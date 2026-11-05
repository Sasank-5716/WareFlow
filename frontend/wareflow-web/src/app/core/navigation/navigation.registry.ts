import { NavigationItem } from './navigation.models';

export const NAVIGATION_REGISTRY: readonly NavigationItem[] = [
  { id: 'overview', label: 'Overview', route: '/app', description: 'Workspace overview', icon: '⌂' },
  { id: 'products', label: 'Products', route: '/app/products', moduleKey: 'products', permission: 'Products.View', description: 'Product catalog', icon: '▦' },
  { id: 'inventory', label: 'Inventory', route: '/app/inventory', moduleKey: 'inventory', permission: 'Inventory.View', description: 'Stock and movements', icon: '▤' },
  { id: 'orders', label: 'Orders', route: '/app/orders', moduleKey: 'orders', permission: 'Orders.View', description: 'Sales orders', icon: '□' },
  { id: 'reports', label: 'Reports & analytics', route: '/app/reports', moduleKey: 'reports_analytics', permission: 'Reports.View', description: 'Operational reporting', icon: '↗' },
  { id: 'settings', label: 'Settings', route: '/app/settings', permission: 'Settings.View', description: 'Workspace settings', icon: '⚙' },
];
