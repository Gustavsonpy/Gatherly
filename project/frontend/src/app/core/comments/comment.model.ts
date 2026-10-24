export interface CommentModel {
    id: string;
    text: string;
    registerDate: string;
    eventId: string;
    userId: string;
    commentUserName: string;
    userPhoto: string | null;
}

export interface CreateCommentModel {
    text: string;
    eventId: string;
}