export interface UserRatingModel {
    id: string;
    rate: number;
    description: string | null;
    ratedUserId: string;
    eventId: string;
}

export interface CreateUserRatingModel {
    rate: number;
    description: string | null;
    ratedUserId: string;
    eventId: string;
}

export interface UserScore {
    userId: string; 
    average: number;
    count: number;
}