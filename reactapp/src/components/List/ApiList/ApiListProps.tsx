interface ApiListProps {
    apis: ApiProps[] | null,
    deleteApi?: (apiId: number) => void
}