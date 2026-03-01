import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box, Container, Typography, Card, CardActionArea, CardContent,
  CardActions, Fab, Chip, IconButton, Dialog, DialogTitle,
  DialogContent, DialogActions, Button, Skeleton, Alert,
  Stack,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import LuggageIcon from '@mui/icons-material/Luggage';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import { api } from '../api/client';
import type { Trip } from '../types/api';
import { TripFormDialog } from '../components/TripFormDialog';

function formatDateRange(start?: string, end?: string): string {
  if (!start && !end) return 'No dates set';
  const fmt = (d: string) => new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  if (start && end) return `${fmt(start)} – ${fmt(end)}`;
  if (start) return `From ${fmt(start)}`;
  return `Until ${fmt(end!)}`;
}

export function TripsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const { data: trips, isLoading, error } = useQuery({
    queryKey: ['trips'],
    queryFn: () => api.trips.list(),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.trips.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['trips'] });
      setDeleteId(null);
    },
  });

  if (error) return <Alert severity="error" sx={{ m: 2 }}>Failed to load trips. Please try again.</Alert>;

  return (
    <Box sx={{ pb: 10 }}>
      {/* Header */}
      <Box sx={{ bgcolor: 'background.paper', borderBottom: '1px solid', borderColor: 'rgba(78,48,37,0.08)', px: 2, py: 2.5 }}>
        <Container maxWidth="sm" disableGutters>
          <Typography variant="h5" fontWeight={700} color="text.primary">My Trips</Typography>
          {!isLoading && trips && (
            <Chip
              label={`${trips.length} trip${trips.length !== 1 ? 's' : ''}`}
              size="small"
              sx={{ mt: 0.5, bgcolor: 'rgba(220,113,71,0.1)', color: 'primary.main', fontWeight: 600 }}
            />
          )}
        </Container>
      </Box>

      <Container maxWidth="sm" sx={{ py: 2, px: 2 }}>
        {isLoading ? (
          <Stack spacing={1.5}>
            {[1, 2, 3].map(i => <Skeleton key={i} variant="rounded" height={100} sx={{ borderRadius: 3 }} />)}
          </Stack>
        ) : trips?.length === 0 ? (
          <Box sx={{ textAlign: 'center', pt: 8, px: 3 }}>
            <LuggageIcon sx={{ fontSize: 64, color: 'rgba(78,48,37,0.15)', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>No trips yet</Typography>
            <Typography variant="body2" color="text.disabled">Tap + to log your first RV trip</Typography>
          </Box>
        ) : (
          <Stack spacing={1.5}>
            {trips?.map((trip: Trip) => (
              <Card key={trip.id} sx={{ position: 'relative' }}>
                <CardActionArea onClick={() => navigate(`/trips/${trip.id}`)}>
                  <CardContent sx={{ pb: 1.5 }}>
                    <Typography variant="subtitle1" fontWeight={700} noWrap>{trip.title}</Typography>
                    <Stack direction="row" spacing={0.5} alignItems="center" sx={{ mt: 0.5, color: 'text.secondary' }}>
                      <CalendarTodayIcon sx={{ fontSize: 13 }} />
                      <Typography variant="caption">{formatDateRange(trip.startDate, trip.endDate)}</Typography>
                    </Stack>
                    {trip.description && (
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.75, display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                        {trip.description}
                      </Typography>
                    )}
                  </CardContent>
                </CardActionArea>
                <CardActions sx={{ pt: 0, px: 2, pb: 1.5, justifyContent: 'space-between' }}>
                  <IconButton size="small" onClick={() => setDeleteId(trip.id)} sx={{ color: 'text.disabled', '&:hover': { color: 'error.main' } }}>
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                  <IconButton size="small" onClick={() => navigate(`/trips/${trip.id}`)} sx={{ color: 'primary.main' }}>
                    <ChevronRightIcon />
                  </IconButton>
                </CardActions>
              </Card>
            ))}
          </Stack>
        )}
      </Container>

      {/* FAB */}
      <Fab
        color="primary"
        aria-label="Add trip"
        onClick={() => setCreateOpen(true)}
        sx={{ position: 'fixed', bottom: { xs: 72, md: 24 }, right: 24 }}
      >
        <AddIcon />
      </Fab>

      {/* Create Trip Dialog */}
      <TripFormDialog open={createOpen} onClose={() => setCreateOpen(false)} />

      {/* Delete Confirm Dialog */}
      <Dialog open={!!deleteId} onClose={() => setDeleteId(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete trip?</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary">This will permanently delete this trip and all its locations.</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteId(null)}>Cancel</Button>
          <Button color="error" variant="contained" onClick={() => deleteId && deleteMutation.mutate(deleteId)} disabled={deleteMutation.isPending}>
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
