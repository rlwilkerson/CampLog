import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  Box, AppBar, Toolbar, Typography, IconButton, Avatar,
  BottomNavigation, BottomNavigationAction, Paper, useMediaQuery,
  Drawer, List, ListItemButton, ListItemIcon, ListItemText, Divider,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import LuggageIcon from '@mui/icons-material/Luggage';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { useAuth } from 'react-oidc-context';

const NAV_ITEMS = [
  { label: 'Trips', value: '/trips', icon: <LuggageIcon /> },
  { label: 'Profile', value: '/profile', icon: <AccountCircleIcon /> },
];

const DRAWER_WIDTH = 220;

export function AppShell() {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const navigate = useNavigate();
  const location = useLocation();
  const auth = useAuth();
  const currentTab = NAV_ITEMS.find(n => location.pathname.startsWith(n.value))?.value ?? '/trips';
  const displayName = auth.user?.profile?.name ?? auth.user?.profile?.preferred_username ?? 'You';

  const navContent = (
    <List disablePadding>
      {NAV_ITEMS.map(item => (
        <ListItemButton
          key={item.value}
          selected={location.pathname.startsWith(item.value)}
          onClick={() => navigate(item.value)}
          sx={{ borderRadius: 2, mx: 1, my: 0.5 }}
        >
          <ListItemIcon sx={{ minWidth: 40, color: location.pathname.startsWith(item.value) ? 'primary.main' : 'inherit' }}>
            {item.icon}
          </ListItemIcon>
          <ListItemText primary={item.label} />
        </ListItemButton>
      ))}
    </List>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      {/* App Bar */}
      <AppBar
        position="fixed"
        color="inherit"
        sx={{ zIndex: theme.zIndex.drawer + 1, bgcolor: 'background.paper' }}
        elevation={0}
      >
        <Toolbar>
          {!isDesktop && (
            <LuggageIcon sx={{ color: 'primary.main', mr: 1.5, fontSize: 28 }} />
          )}
          <Typography
            variant="h6"
            sx={{ fontWeight: 700, color: 'primary.main', letterSpacing: '-0.02em', flexGrow: 1 }}
          >
            CampLog
          </Typography>
          <IconButton size="small">
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main', fontSize: 14 }}>
              {displayName.charAt(0).toUpperCase()}
            </Avatar>
          </IconButton>
        </Toolbar>
      </AppBar>

      {/* Desktop Sidebar */}
      {isDesktop && (
        <Drawer
          variant="permanent"
          sx={{
            width: DRAWER_WIDTH,
            flexShrink: 0,
            '& .MuiDrawer-paper': {
              width: DRAWER_WIDTH,
              boxSizing: 'border-box',
              bgcolor: 'background.paper',
              borderRight: '1px solid',
              borderColor: 'rgba(78, 48, 37, 0.08)',
              pt: '64px',
            },
          }}
        >
          <Box sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
              <LuggageIcon sx={{ color: 'primary.main', fontSize: 28 }} />
              <Typography variant="h6" sx={{ fontWeight: 700, color: 'primary.main' }}>CampLog</Typography>
            </Box>
            <Divider sx={{ mb: 1 }} />
            {navContent}
          </Box>
        </Drawer>
      )}

      {/* Main content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          pt: '64px',
          pb: isDesktop ? 0 : '56px',
          ml: isDesktop ? `${DRAWER_WIDTH}px` : 0,
          minHeight: '100vh',
          maxWidth: isDesktop ? 'none' : '100%',
        }}
      >
        <Outlet />
      </Box>

      {/* Mobile Bottom Navigation */}
      {!isDesktop && (
        <Paper
          sx={{ position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: theme.zIndex.appBar }}
          elevation={3}
        >
          <BottomNavigation value={currentTab} onChange={(_, v) => navigate(v)} showLabels>
            {NAV_ITEMS.map(item => (
              <BottomNavigationAction key={item.value} label={item.label} value={item.value} icon={item.icon} />
            ))}
          </BottomNavigation>
        </Paper>
      )}
    </Box>
  );
}
