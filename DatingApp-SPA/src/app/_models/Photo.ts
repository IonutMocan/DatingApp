export interface Photo {
    id: number;
    url: string;
    description: string;
    dateAdded: Date;
    isMain: boolean;
    isApproved: boolean; // De asemenea si aceasta interfata trebuie updatata
}
