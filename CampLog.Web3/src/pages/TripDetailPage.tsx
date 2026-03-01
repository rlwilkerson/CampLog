import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box, Container, Typography, Card, CardContent, CardActions,
  Fab, IconButton, Dialog, DialogTitle, DialogContent, DialogActions,
  Button, Chip, Skeleton, Alert, Stack,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import PlaceIcon from '@mui/icons-material/Place';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import { api } from '../api/client';
import { TripFormDialog } from '../components/TripFormDialog';
import { LocationFormDialog } from '../components/LocationFormDialog';
import type { Location } from '../types/api';

export function TripDetailPage() {
  const { tripId } = useParams<{ tripId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [editTripOpen, setEditTripOpen] = useState(false);
  const [addLocationOpen, setAddLocationOpen] = useState(false);
  const [deleteLocId, setDeleteLocId] = useState<string | null>(null);

  const { data: trip, isLoading: tripLoading, error: tripError } = useQuery({
    queryKey: ['trips', tripId],
    queryFn: () => api.trips.get(tripId!),
    enabled: !!tripId,
  });

  const { data: locations, isLoading: locsLoading } = useQuery({
    queryKey: ['trips', tripId, 'locations'],
    queryFn: () => api.locations.list(tripId!),
    enabled: !!tripId,
  });

  const deleteLocMutation = useMutation({
    mutationFn: (id: string) => api.locations.delete(tripId!, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['trips', tripId, 'locations'] });
      setDeleteLocId(null);
    },
  });

  if (tripError) return <Alert severity="error" sx={{ m: 2 }}>Failed to load trip.</Alert>;

  return (
    <Box sx={{ pb: 10 }}>
      {/* Inner app bar for back navigation */}
      <Box sx={{ bgcolor: 'background.paper', borderBottom: '1px solid', borderColor: 'rgba(78,48,37,0.08)', px: 2, py: 2 }}>
        <Container maxWidth="sm" disableGutters>
          <Stack direction="row" alignItems="center" spacing={1}>
            <IconButton size="small" onClick={() => navigate('/trips')} sx={{ color: 'primary.main' }}>
              <ArrowBackIcon />
            </IconButton>
            {tripLoading ? (
              <Skeleton width={160} height={28} />
            ) : (
              <Typography variant="h6" fontWeight={700} noWrap sx={{ flex: 1 }}>{trip?.title}</Typography>
            )}
            <IconButton size="small" onClick={() => setEditTripOpen(true)} sx={{ color: 'text.secondary' }}>
              <EditOutlinedIcon fontSize="small" />
            </IconButton>
          </Stack>
        </Container>
      </Box>

      <Container maxWidth="sm" sx={{ py: 2, px: 2 }}>
        <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" fontWeight={600}>
            LOCATIONS
          </Typography>
          {!locsLoading && locations && (
            <Chip label={locations.length} size="small" sx={{ bgcolor: 'rgba(220,113,71,0.1)', color: 'primary.main', fontWeight: 700 }} />
          )}
        </Stack>

        {locsLoading ? (
          <Stack spacing={1.5}>
            {[1, 2].map(i => <Skeleton key={i} variant="rounded" height={80} sx={{ borderRadius: 3 }} />)}
          </Stack>
        ) : locations?.length === 0 ? (
          <Box sx={{ textAlign: 'center', pt: 6, px: 3 }}>
            <PlaceIcon sx={{ fontSize: 48, color: 'rgba(78,48,37,0.15)', mb: 1.5 }} />
            <Typography variant="body2" color="text.secondary">No locations yet — tap + to add one</Typography>
          </Box>
        ) : (
          <Stack spacing={1.5}>
            {locations?.map((loc: Location) => (
              <Card key={loc.id}>
                <CardContent sx={{ pb: 1 }}>
                  <Stack direction="row" spacing={1} alignItems="flex-start">
                    <PlaceIcon sx={{ color: 'primary.main', mt: 0.3, fontSize: 20 }} />
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle2" fontWeight={700}>{loc.name}</Typography>
                      {(loc.arrivalDate || loc.departureDate) && (
                        <Typography variant="caption" color="text.secondary">
                          {[loc.arrivalDate, loc.departureDate].filter(Boolean).join(' – ')}
                        </Typography>
                      )}
                      {loc.notes && (
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{loc.notes}</Typography>
                      )}
                    </Box>
                  </Stack>
                </CardContent>
                <CardActions sx={{ pt: 0, px: 2, pb: 1.5 }}>
                  <IconButton size="small" onClick={() => setDeleteLocId(loc.id)} sx={{ color: 'text.disabled', '&:hover': { color: 'error.main' } }}>
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </CardActions>
              </Card>
            ))}
          </Stack>
        )}
      </Container>

      <Fab
        color="primary"
        aria-label="Add location"
        onClick={() => setAddLocationOpen(true)}
        sx={{ position: 'fixed', bottom: { xs: 72, md: 24 }, right: 24 }}
      >
        <AddIcon />
      </Fab>

      {trip && <TripFormDialog open={editTripOpen} onClose={() => setEditTripOpen(false)} trip={trip} />}
      <LocationFormDialog open={addLocationOpen} onClose={() => setAddLocationOpen(false)} tripId={tripId!} />

      <Dialog open={!!deleteLocId} onClose={() => setDeleteLocId(null)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete location?</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary">This location will be permanently removed from the trip.</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteLocId(null)}>Cancel</Button>
          <Button color="error" variant="contained" onClick={() => deleteLocId && deleteLocMutation.mutate(deleteLocId)} disabled={deleteLocMutation.isPending}>Delete</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
