export type Worker = {
  workerID: number,
  isActive: boolean,
  workerName: string,
  detailedDescription: string,
  emailOnSuccess: string,
  parentWorkerID?: number | null,
  timeoutMinutes: number,
  directoryName: string,
  executable: string,
  argumentValues: string
};
