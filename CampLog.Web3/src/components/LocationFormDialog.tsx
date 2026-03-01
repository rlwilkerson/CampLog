import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient, useMutation } from '@tanstack/react-query';
import { Dialog, DialogTitle, DialogContent, DialogActions, Button, TextField, Stack } from '@mui/material';
import { api } from '../api/client';
import type { Location } from '../types/api';

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  arrivalDate: z.string().optional(),
  departureDate: z.string().optional(),
  notes: z.string().max(500).optional(),
});
type FormValues = z.infer<typeof schema>;

interface Props { open: boolean; onClose: () => void; tripId: string; location?: Location; }

export function LocationFormDialog({ open, onClose, tripId, location }: Props) {
  const queryClient = useQueryClient();
  const isEditing = !!location;
  const { control, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', arrivalDate: '', departureDate: '', notes: '' },
  });

  useEffect(() => {
    if (open) reset(location ? { name: location.name, arrivalDate: location.arrivalDate ?? '', departureDate: location.departureDate ?? '', notes: location.notes ?? '' } : { name: '', arrivalDate: '', departureDate: '', notes: '' });
  }, [open, location, reset]);

  const mutation = useMutation({
    mutationFn: (data: FormValues) => {
      const payload = { ...data, arrivalDate: data.arrivalDate || undefined, departureDate: data.departureDate || undefined, notes: data.notes || undefined };
      return isEditing ? api.locations.update(tripId, location.id, payload) : api.locations.create(tripId, payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['trips', tripId, 'locations'] });
      onClose();
    },
  });

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(d => mutation.mutate(d))}>
        <DialogTitle sx={{ fontWeight: 700 }}>{isEditing ? 'Edit Location' : 'Add Location'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2.5} sx={{ mt: 1 }}>
            <Controller name="name" control={control} render={({ field }) => (
              <TextField {...field} label="Location name" fullWidth error={!!errors.name} helperText={errors.name?.message} autoFocus />
            )} />
            <Controller name="arrivalDate" control={control} render={({ field }) => (
              <TextField {...field} label="Arrival date" type="date" fullWidth InputLabelProps={{ shrink: true }} />
            )} />
            <Controller name="departureDate" control={control} render={({ field }) => (
              <TextField {...field} label="Departure date" type="date" fullWidth InputLabelProps={{ shrink: true }} />
            )} />
            <Controller name="notes" control={control} render={({ field }) => (
              <TextField {...field} label="Notes" multiline rows={3} fullWidth />
            )} />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained" disabled={mutation.isPending}>
            {isEditing ? 'Save' : 'Add location'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
