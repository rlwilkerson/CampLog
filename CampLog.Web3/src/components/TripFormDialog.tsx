import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient, useMutation } from '@tanstack/react-query';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Stack,
} from '@mui/material';
import { api } from '../api/client';
import type { Trip } from '../types/api';

const schema = z.object({
  title: z.string().min(1, 'Title is required').max(100),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  description: z.string().max(500).optional(),
});

type FormValues = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onClose: () => void;
  trip?: Trip;
}

export function TripFormDialog({ open, onClose, trip }: Props) {
  const queryClient = useQueryClient();
  const isEditing = !!trip;

  const { control, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { title: '', startDate: '', endDate: '', description: '' },
  });

  useEffect(() => {
    if (open) {
      reset(trip ? {
        title: trip.title,
        startDate: trip.startDate ?? '',
        endDate: trip.endDate ?? '',
        description: trip.description ?? '',
      } : { title: '', startDate: '', endDate: '', description: '' });
    }
  }, [open, trip, reset]);

  const mutation = useMutation({
    mutationFn: (data: FormValues) => {
      const payload = { ...data, startDate: data.startDate || undefined, endDate: data.endDate || undefined, description: data.description || undefined };
      return isEditing ? api.trips.update(trip.id, payload) : api.trips.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['trips'] });
      onClose();
    },
  });

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth fullScreen={false}>
      <form onSubmit={handleSubmit(d => mutation.mutate(d))}>
        <DialogTitle sx={{ fontWeight: 700 }}>{isEditing ? 'Edit Trip' : 'New Trip'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2.5} sx={{ mt: 1 }}>
            <Controller name="title" control={control} render={({ field }) => (
              <TextField {...field} label="Trip title" fullWidth error={!!errors.title} helperText={errors.title?.message} autoFocus />
            )} />
            <Controller name="startDate" control={control} render={({ field }) => (
              <TextField {...field} label="Start date" type="date" fullWidth InputLabelProps={{ shrink: true }} />
            )} />
            <Controller name="endDate" control={control} render={({ field }) => (
              <TextField {...field} label="End date" type="date" fullWidth InputLabelProps={{ shrink: true }} />
            )} />
            <Controller name="description" control={control} render={({ field }) => (
              <TextField {...field} label="Description" multiline rows={3} fullWidth />
            )} />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained" disabled={mutation.isPending}>
            {isEditing ? 'Save changes' : 'Create trip'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
