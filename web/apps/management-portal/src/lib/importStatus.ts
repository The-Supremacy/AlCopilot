export function formatImportBatchStatus(status: string) {
  switch (status) {
    case 'InProgress':
      return 'In progress';
    case 'Completed':
      return 'Completed';
    case 'Cancelled':
      return 'Cancelled';
    default:
      return status;
  }
}
