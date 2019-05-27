;;by 天华AI研究中心
;;天华效率工具平台-通用工具-辅助工具

;Z0:Z轴归零：将整幅图所有图元的标高值即z坐标归零
(defun C:THZ0();z轴归零
  (setvar "cmdecho" 0)
  (princ "\n正在处理图形数据,请稍候...")
  (command "_.UCS" "");恢复默认坐标系
  (command "_.move" "_all" "" '(0 0 1e99) "" "_.move" "_p" "" '(0 0 -1e99) "")
  (princ "\nOK, 已将所有图元的标高值即Z坐标归零.")
  (setvar "cmdecho" 1)
  (princ)
)
