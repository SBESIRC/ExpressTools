;;by �컪AI�о�����
;;�컪Ч�ʹ���ƽ̨-ͨ�ù���-��������

;Z0:Z����㣺������ͼ����ͼԪ�ı��ֵ��z�������
(defun C:THZ0();z�����
  (setvar "cmdecho" 0)
  (princ "\n���ڴ���ͼ������,���Ժ�...")
  (command "_.UCS" "");�ָ�Ĭ������ϵ
  (command "_.move" "_all" "" '(0 0 1e99) "" "_.move" "_p" "" '(0 0 -1e99) "")
  (princ "\nOK, �ѽ�����ͼԪ�ı��ֵ��Z�������.")
  (setvar "cmdecho" 1)
  (princ)
)
