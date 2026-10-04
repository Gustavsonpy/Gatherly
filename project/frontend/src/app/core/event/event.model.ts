export interface EventModel{
    id: string;
    title: string;
    description: string;
    dateTime: string;
    date: string;
    time: string;
    localization: string;
    maxCapacity: number;
    city: string;
    urlImage: string;
    registerDate: string;
    userId: string;
    categoryId: string;
}

export interface CreateEventModel {
    title: string;
    description: string;
    dateTime: string;
    localization: string;
    maxCapacity: number;
    city: string;
    level: string | null;
    urlImage: string;
    categoryId: string;
}