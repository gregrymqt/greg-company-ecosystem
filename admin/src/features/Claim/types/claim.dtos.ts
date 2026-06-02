import type { FieldValues } from "react-hook-form";

export interface ReplyFormData extends FieldValues {
  message: string;
  attachments: FileList;
}
