export interface Trip {
  id: string;
  title: string;
  startDate?: string;
  endDate?: string;
  description?: string;
}

export interface Location {
  id: string;
  tripId: string;
  name: string;
  arrivalDate?: string;
  departureDate?: string;
  notes?: string;
}

export interface CreateTripRequest {
  title: string;
  startDate?: string;
  endDate?: string;
  description?: string;
}

export interface UpdateTripRequest extends CreateTripRequest {}

export interface CreateLocationRequest {
  name: string;
  arrivalDate?: string;
  departureDate?: string;
  notes?: string;
}

export interface UpdateLocationRequest extends CreateLocationRequest {}
