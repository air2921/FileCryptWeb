interface OfferListProps {
    offers: OfferProps[] | null,
    isOwner: boolean,
    deleteOffer?: (offerId: number) => void,
    acceptOffer?: (offerId: number) => void,
    error?: string
}