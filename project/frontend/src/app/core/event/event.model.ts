import { PlaceDetails } from '../places/place-provider';

export interface EventModel extends PlaceDetails {
    id: string;
    title: string;
    description: string;
    dateTimeStart: string;
    dateTimeEnd: string;
    localization: string;
    maxCapacity: number;
    city: string;
    urlImage: string;
    registerDate: string;
    userId: string;
    categoryId: string;
    categoryName: string;
    participantsCount: number;
    isParticipating: boolean;
    creatorName: string | null;
    creatorPhoto: string | null;
    status: 'upcoming' | 'ongoing' | 'done' | 'canceled';
}

export interface CreateEventModel extends PlaceDetails {
    title: string;
    description: string;
    dateTimeStart: string;
    dateTimeEnd: string;
    localization: string;
    maxCapacity: number;
    city: string;
    level: string | null;
    urlImage: string;
    categoryId: string;
}

export interface ParticipantModel {
    id: string;
    name: string;
    urlPhoto: string | null;
    userScore: number;
    ratingsCount: number;
}