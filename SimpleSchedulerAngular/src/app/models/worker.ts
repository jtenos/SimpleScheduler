export default class Worker {
  constructor(public workerID: number, public isActive: boolean, public description: string,
              public timeoutMinutes: number, public overdueMinutes: number,
              public freeText: string, public emailOnSuccess: string, public directoryName: string,
              public executable: string, public argumentValues: string,  public parentWorkerID?: number) {
  }
}
