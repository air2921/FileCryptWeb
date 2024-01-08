interface OfferListProps {
    offers: OfferProps[] | null,
    isOwner: boolean,
    user_id?: number
    deleteOffer?: (offerId: number) => void,
    acceptOffer?: (offerId: number) => void,
    error?: string
}