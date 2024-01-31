interface FileListProps {
    files: FileProps[] | null,
    isOwner: boolean,
    deleteFile?: (offerId: number) => void,
} 