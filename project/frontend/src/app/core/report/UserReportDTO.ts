export interface UserInterfaceDTO {
    id: string, 
    reporterId: string,
    reportedUserId: string,
    eventId: string,
    reason: number,
    customReason: string | null,
    description: string,
    status: string,
    createdAt: string
}